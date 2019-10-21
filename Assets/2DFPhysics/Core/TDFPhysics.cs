using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

namespace TDFP.Core
{
    public class TDFPhysics : MonoBehaviour
    {
        public static TDFPhysics instance;
        public static List<FPRigidbody> bodies = new List<FPRigidbody>();

        [HideInInspector] public Fix resting;
        [HideInInspector] public Fix penetrationAllowance = (Fix)0.05f;
        [HideInInspector] public Fix penetrationCorrection = (Fix)0.4f;

        public TDFPSettings settings;

        //Broad Phase
        private List<Manifold> broadPhasePairs = new List<Manifold>();
        private List<Manifold> narrowPhasePairs = new List<Manifold>();

        private void Awake()
        {
            instance = this;

            //Init variables.
            resting = (settings.gravity * settings.deltaTime).GetMagnitudeSquared() + Fix.Epsilon;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                UpdatePhysics(settings.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (settings.AutoSimulation)
            {
                UpdatePhysics(settings.deltaTime);
            }
        }

        void TimeStep()
        {

        }

        public void UpdatePhysics(Fix dt)
        {
            //Profiler.BeginSample("Physics Update");
            BroadPhase();
            NarrowPhase();
            //Profiler.EndSample();
        }

        #region Broad Phase
        private void BroadPhase()
        {
            broadPhasePairs.Clear();
            for (int i = 0; i < bodies.Count; i++)
            {
                for (int w = 0; w < bodies.Count; w++)
                {
                    //If it's the same body, ignore it.
                    if (bodies[i] == bodies[w])
                    {
                        continue;
                    }

                    if (bodies[i].invMass == 0 && bodies[w].invMass == 0)
                    {
                        continue;
                    }

                    if (CollisionChecks.AABBvsAABB(new Manifold(bodies[i], bodies[w])) == true)
                    {
                        broadPhasePairs.Add(new Manifold(bodies[i], bodies[w]));
                    }
                }
            }
            CullDups();
            SortPairs();
        }

        private void CullDups()
        {
            for(int i = 0; i < broadPhasePairs.Count; i++)
            {
                for(int w = 0; w < broadPhasePairs.Count; w++)
                {
                    if(i == w)
                    {
                        continue;
                    }

                    if(broadPhasePairs[i].A == broadPhasePairs[w].A && broadPhasePairs[i].B == broadPhasePairs[w].B
                        || broadPhasePairs[i].A == broadPhasePairs[w].B && broadPhasePairs[i].B == broadPhasePairs[w].A)
                    {
                        broadPhasePairs.RemoveAt(w);
                    }
                }
            }
        }

        ManifoldComparer mc = new ManifoldComparer();
        private void SortPairs()
        {
            broadPhasePairs.Sort(mc);
        }
        #endregion

        #region Narrow Phase
        private void NarrowPhase()
        {
            narrowPhasePairs.Clear();
            for(int i = 0; i < broadPhasePairs.Count; i++)
            {
                broadPhasePairs[i].solve();

                if (broadPhasePairs[i].contactCount > 0)
                {
                    broadPhasePairs[i].A.currentlyCollidingWith.Add(broadPhasePairs[i].B.coll);
                    broadPhasePairs[i].B.currentlyCollidingWith.Add(broadPhasePairs[i].A.coll);
                    //If either are a trigger, just exit out.
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
                IntegrateForces(bodies[i], settings.deltaTime);
            }

            // Initialize collision
            for (int i = 0; i < narrowPhasePairs.Count; ++i)
            {
                narrowPhasePairs[i].initialize();
            }

            // Solve collisions
            for (int j = 0; j < settings.solveCollisionIterations; ++j)
            {
                for (int i = 0; i < narrowPhasePairs.Count; ++i)
                {
                    narrowPhasePairs[i].ApplyImpulse();
                }
            }

            // Integrate velocities
            for (int i = 0; i < bodies.Count; ++i)
            {
                IntegrateVelocity(bodies[i], settings.deltaTime);
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
                //Handle events
                b.HandlePhysicsEvents();
            }
        }

        private void IntegrateVelocity(FPRigidbody b, Fix dt)
        {
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
            if (b.invMass == Fix.Zero)
                return;

            b.info.velocity += ((b.info.force * b.invMass) + (settings.gravity * b.gravityScale)) * (dt / (Fix.One+Fix.One));
            b.info.angularVelocity += b.info.torque * b.invInertia * (dt / (Fix.One+Fix.One));
        }
        #endregion

        #region Physics Checks
        public bool BiasGreaterThan(Fix a, Fix b)
        {
            return a >= b * settings.biasRelative + a * settings.biasAbsolute;
        }
        #endregion
    }
}