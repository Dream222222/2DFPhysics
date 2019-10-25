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
        private static TDFPPhysicsScene physicsScene = new TDFPPhysicsScene();

        [HideInInspector] public Fix resting;
        [HideInInspector] public Fix penetrationAllowance = (Fix)0.05f;
        [HideInInspector] public Fix penetrationCorrection = (Fix)0.4f;

        public TDFPSettings settings;

        private void Awake()
        {
            physicsScene.spatialGrid = new SpatialGrid(settings.gridMinPosition,
                settings.gridMaxPosition, settings.gridCellSize);
            instance = this;
            resting = (settings.gravity * settings.deltaTime).GetMagnitudeSquared() + Fix.Epsilon;
        }

        private void FixedUpdate()
        {
            if (settings.AutoSimulation)
            {
                UpdatePhysics(settings.deltaTime);
            }
        }

        public void UpdatePhysics(Fix dt)
        {
            physicsScene.Update();
        }

        /// <summary>
        /// Adds a body to the simulation.
        /// </summary>
        /// <param name="body"></param>
        public static void AddBody(FPRigidbody body)
        {
            physicsScene.AddBody(body);
        }

        /// <summary>
        /// Removes a body from the simulation.
        /// </summary>
        /// <param name="body"></param>
        public static void RemoveBody(FPRigidbody body)
        {
            physicsScene.RemoveBody(body);
        }

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