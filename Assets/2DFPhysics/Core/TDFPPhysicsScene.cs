using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

namespace TDFP.Core
{
    [Serializable]
    public class TDFPPhysicsScene
    {
        public List<FPRigidbody> bodies = new List<FPRigidbody>();
        public List<Manifold> broadPhasePairs = new List<Manifold>();
        public List<Manifold> narrowPhasePairs = new List<Manifold>();

        public DynamicTree dynamicTree = new DynamicTree();

        public void AddBody(FPRigidbody body, Fix aabbFattening)
        {
            bodies.Add(body);
            body.ProxyID = dynamicTree.CreateProxy(body.bounds, bodies.Count-1, aabbFattening);
        }

        public void RemoveBody(FPRigidbody body)
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
            GetPairs();
            NarrowPhase();
        }

        #region Broad Phase
        private void GetPairs()
        {
            broadPhasePairs.Clear();
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
                    // If either are a trigger, just exit out.
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
                IntegrateForces(bodies[i], TDFPhysics.instance.settings.deltaTime);
            }

            // Initialize collision
            for (int i = 0; i < narrowPhasePairs.Count; ++i)
            {
                narrowPhasePairs[i].initialize();
            }

            // Solve collisions
            for (int j = 0; j < TDFPhysics.instance.settings.solveCollisionIterations; ++j)
            {
                for (int i = 0; i < narrowPhasePairs.Count; ++i)
                {
                    narrowPhasePairs[i].ApplyImpulse();
                }
            }

            // Integrate velocities
            for (int i = 0; i < bodies.Count; ++i)
            {
                IntegrateVelocity(bodies[i], TDFPhysics.instance.settings.deltaTime);
            }

            // Correct positions
            for (int i = 0; i < narrowPhasePairs.Count; ++i)
            {
                narrowPhasePairs[i].PositionalCorrection();
            }

            for (int i = 0; i < bodies.Count; ++i)
            {
                // Clear all forces
                FPRigidbody b = bodies[i];
                b.info.force = new FixVec2(0, 0);
                b.info.torque = 0;
                // Handle events
                b.HandlePhysicsEvents();
            }
        }

        private void IntegrateVelocity(FPRigidbody b, Fix dt)
        {
            // If the body is static, ignore it.
            if (b.invMass == Fix.Zero)
            {
                return;
            }
            b.Position += b.info.velocity * dt;
            b.info.rotation += b.info.angularVelocity * dt;
            b.SetRotation(b.info.rotation);
            IntegrateForces(b, dt);
        }

        private void IntegrateForces(FPRigidbody b, Fix dt)
        {
            // If the body is static, ignore it.
            if (b.invMass == Fix.Zero)
            {
                return;
            }
            b.info.velocity += ((b.info.force * b.invMass) + (TDFPhysics.instance.settings.gravity * b.gravityScale)) * (dt / (Fix.One + Fix.One));
            b.info.angularVelocity += b.info.torque * b.invInertia * (dt / (Fix.One + Fix.One));
        }
        #endregion
    }
}
