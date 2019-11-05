using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

namespace TF.Core
{
    [Serializable]
    public class TFPhysicsScene : ITreeQueryCallback, ITreeRaycastCallback
    {
        public List<TFRigidbody> bodies = new List<TFRigidbody>();

        // Broad Phase
        private ManifoldComparer manifoldComparer = new ManifoldComparer();

        public DynamicTree dynamicTree = new DynamicTree();
        public List<Manifold> broadPhasePairs = new List<Manifold>();
        public List<int> movedBodies = new List<int>(); // The bodies that have moved since we last checked for pairs.

        // Narrow Phase
        public List<Manifold> narrowPhasePairs = new List<Manifold>();

        public void AddBody(TFRigidbody body, Fix aabbFattening)
        {
            bodies.Add(body);
            body.ProxyID = dynamicTree.CreateProxy(body.bounds, bodies.Count-1, aabbFattening);
            MoveProxy(body.ProxyID);
        }

        public void RemoveBody(TFRigidbody body)
        {
            int index = bodies.IndexOf(body);
            if (index > -1)
            {
                bodies.RemoveAt(index);
                dynamicTree.DestroyProxy(body.ProxyID);
            }
        }

        public void Step()
        {
            BroadPhase();
            NarrowPhase();
        }

        /// <summary>
        /// Call whenever we move a rigidbody, updating it's representation
        /// in the dynamic tree.
        /// </summary>
        /// <param name="proxyId"></param>
        /// <param name="aabb"></param>
        /// <param name="displacement"></param>
        private void MoveProxy(int proxyId, AABB sweptAABB, FixVec2 displacement)
        {
            bool hasMoved = dynamicTree.MoveProxy(proxyId, sweptAABB, displacement);
            //if (hasMoved)
            //{
            MoveProxy(proxyId);
            //}
        }

        private void MoveProxy(int proxyId)
        {
            movedBodies.Add(proxyId);
        }

        #region Broad Phase
        int getPairsProxyID;

        private void BroadPhase()
        {
            broadPhasePairs.Clear();
            GetPairs();
        }

        private void GetPairs()
        {
            // Perform tree queries for all proxies that have moved.
            for (int j = 0; j < movedBodies.Count; j++)
            {
                getPairsProxyID = movedBodies[j];
                if (getPairsProxyID == -1)
                {
                    continue;
                }

                // We have to query the tree with the fat AABB so that
                // we don't fail to create a pair that may touch later.
                AABB fatAABB = dynamicTree.GetFatAABB(getPairsProxyID);

                // Query tree, create pairs and add them pair buffer.
                dynamicTree.Query(this, fatAABB);
            }
            movedBodies.Clear();

           broadPhasePairs.Sort(0, broadPhasePairs.Count, manifoldComparer);
           // Remove duplicates.
           for(int i = 0; i < broadPhasePairs.Count-1; i++)
            {
                if(broadPhasePairs[i].A != broadPhasePairs[i+1].A
                    || broadPhasePairs[i].B != broadPhasePairs[i + 1].B)
                {
                    continue;
                }
                // They're the same, remove one.
                broadPhasePairs.RemoveAt(i);
            }
        }

        // This is called from DynamicTree.Query when we are gathering pairs.
        public bool QueryCallback(int proxyID)
        {
            if(proxyID == -1)
            {
                return true;
            }
            // A proxy cannot form a pair with itself.
            if (proxyID == getPairsProxyID)
            {
                return true;
            }

            int A = Mathf.Min(proxyID, getPairsProxyID);
            int B = Mathf.Max(proxyID, getPairsProxyID);

            broadPhasePairs.Add(new Manifold(bodies[dynamicTree.nodes[A].bodyIndex], bodies[dynamicTree.nodes[B].bodyIndex]));
            return true;
        }
        #endregion

        #region Narrow Phase
        private void NarrowPhase()
        {
            narrowPhasePairs.Clear();
            for (int i = 0; i < broadPhasePairs.Count; i++)
            {
                broadPhasePairs[i].solve();

                if (broadPhasePairs[i].contactCount > 0)
                {
                    broadPhasePairs[i].A.currentlyCollidingWith.Add(broadPhasePairs[i].B.coll);
                    broadPhasePairs[i].B.currentlyCollidingWith.Add(broadPhasePairs[i].A.coll);
                    // If either are a trigger, don't bother.
                    if (broadPhasePairs[i].A.coll.isTrigger
                        || broadPhasePairs[i].B.coll.isTrigger)
                    {
                        continue;
                    }
                    narrowPhasePairs.Add(broadPhasePairs[i]);
                }
            }

            // Integrate forces
            for (int i = 0; i < bodies.Count; ++i)
            {
                IntegrateForces(bodies[i], TFPhysics.instance.settings.deltaTime);
            }

            // Initialize collision
            for (int i = 0; i < narrowPhasePairs.Count; ++i)
            {
                narrowPhasePairs[i].initialize();
            }

            // Solve collisions
            for (int j = 0; j < TFPhysics.instance.settings.solveCollisionIterations; ++j)
            {
                for (int i = 0; i < narrowPhasePairs.Count; ++i)
                {
                    narrowPhasePairs[i].ApplyImpulse();
                }
            }

            // Integrate velocities
            for (int i = 0; i < bodies.Count; ++i)
            {
                IntegrateVelocity(bodies[i], TFPhysics.instance.settings.deltaTime);
            }

            // Correct positions
            for (int i = 0; i < narrowPhasePairs.Count; ++i)
            {
                narrowPhasePairs[i].PositionalCorrection();
            }

            for (int i = 0; i < bodies.Count; ++i)
            {
                // Clear all forces
                TFRigidbody b = bodies[i];
                b.info.force = new FixVec2(0, 0);
                b.info.torque = 0;
                // Handle events
                b.HandlePhysicsEvents();
            }
        }

        private void IntegrateForces(TFRigidbody b, Fix dt)
        {
            // If the body is static or kinematic, don't apply any forces.
            if (b.bodyType == TFBodyType.Static
                || b.bodyType == TFBodyType.Kinematic)
            {
                return;
            }
            b.info.velocity += ((b.info.force * b.invMass) + (TFPhysics.instance.settings.gravity * b.gravityScale)) * (dt / 2);
            b.info.angularVelocity += b.info.torque * b.invInertia * (dt / 2);
        }

        private void IntegrateVelocity(TFRigidbody b, Fix dt)
        {
            // If the body is static, don't move it.
            if (b.bodyType == TFBodyType.Static)
            {
                return;
            }
            AABB oldAABB = b.bounds;
            FixVec2 offset = b.info.velocity * dt;
            b.Position += offset;
            b.info.rotation += b.info.angularVelocity * dt;
            b.SetRotation(b.info.rotation);
            IntegrateForces(b, dt);

            // Update the dynamic tree (for Broad Phase).
            AABB sweptAABB = AABB.Union(b.bounds, oldAABB);
            MoveProxy(b.ProxyID, sweptAABB, offset);
        }
        #endregion

        /// <summary>
        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="aabb"></param>
        public void Query(ITreeQueryCallback callback, AABB aabb)
        {
            dynamicTree.Query(callback, aabb);
        }

        public bool Raycast(FixVec2 pointA, FixVec2 pointB, out TFRaycastHit2D hit)
        {
            hit = default;
            dynamicTree.Raycast(this, pointA, pointB);
            return false;
        }

        public Fix RayCastCallback(FixVec2 pointA, FixVec2 pointB, Fix maxFraction, int proxyID)
        {
            bool hit = false;
            TFRigidbody rigid = bodies[dynamicTree.nodes[proxyID].bodyIndex];
            TFRaycastHit2D rHit;
            hit = rigid.coll.Raycast(out rHit, pointA, pointB, maxFraction);

            if (!hit)
            {
                // We did not hit the body, ignore it and use our max ray length.
                return maxFraction;
            }

            Fix fraction = rHit.fraction;
            FixVec2 point = (Fix.one - fraction) * pointA + fraction * pointB;
            return Fix.zero;
        }
    }
}
