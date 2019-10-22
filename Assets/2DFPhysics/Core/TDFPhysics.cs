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
        public static List<Manifold> broadPhasePairs = new List<Manifold>();

        [HideInInspector] public Fix resting;
        [HideInInspector] public Fix penetrationAllowance = (Fix)0.05f;
        [HideInInspector] public Fix penetrationCorrection = (Fix)0.4f;

        public TDFPSettings settings;

        private SpatialGrid spatialGrid;
        private List<Manifold> narrowPhasePairs = new List<Manifold>();

        private void Awake()
        {
            instance = this;

            //Init variables.
            resting = (settings.gravity * settings.deltaTime).GetMagnitudeSquared() + Fix.Epsilon;
            spatialGrid = new SpatialGrid(settings.gridMinPosition, settings.gridMaxPosition, settings.gridCellSize);
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
            broadPhasePairs.Clear();
            spatialGrid.Update();
            NarrowPhase();
        }

        #region Broad Phase
        /*
        private void BroadPhase()
        {
            broadPhasePairs.Clear();
            for (int i = 0; i < bodies.Count; i++)
            {
                // Body isn't in the simulation, ignore it.
                if (!bodies[i].simulated)
                {
                    continue;
                }

                for (int w = i+1; w < bodies.Count; w++)
                {
                    // Body isn't in the simulation, ignore it.
                    if(!bodies[w].simulated)
                    {
                        continue;
                    }

                    // If both bodies are static, ignore them.
                    if (bodies[i].invMass == 0 && bodies[w].invMass == 0)
                    {
                        continue;
                    }

                    if (CollisionChecks.AABBvsAABB(bodies[i].coll, bodies[w].coll))
                    {
                        broadPhasePairs.Add(new Manifold(bodies[i], bodies[w]));
                    }
                }
            }
        }*/
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

        #region Physics
        public bool Raycast(FixVec2 origin, FixVec2 firection, Fix maxDistance)
        {
            return false;
        }
        #endregion
    }
}