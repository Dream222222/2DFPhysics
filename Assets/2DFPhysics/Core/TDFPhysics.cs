using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDFP.Core
{
    public class TDFPhysics : MonoBehaviour
    {
        public static TDFPhysics instance;
        public static TDFPPhysicsScene physicsScene = new TDFPPhysicsScene();

        [HideInInspector] public Fix resting;
        [HideInInspector] public Fix penetrationAllowance = (Fix)0.05f;
        [HideInInspector] public Fix penetrationCorrection = (Fix)0.4f;

        public TDFPSettings settings;

        public bool debug;

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
            physicsScene.AddBody(body, (Fix)0.2f);
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

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!debug)
            {
                return;
            }

            DynamicTree dt = physicsScene.dynamicTree;
            if (dt == null)
            {
                return;
            }

            for(int i = 0; i < dt.nodeCount; i++)
            {
                DTNode node = dt.nodes[i];
                if(i == dt.rootIndex)
                {
                    Handles.color = Color.white;
                }else if (node.IsLeaf())
                {
                    Handles.color = Color.green;
                }
                else
                {
                    Handles.color = Color.yellow;
                }
                Handles.DrawLine((Vector3)(node.aabb.min), (Vector3)(new FixVec3(node.aabb.max.X, node.aabb.min.Y, 0)) );
                Handles.DrawLine((Vector3)(new FixVec3(node.aabb.max.X, node.aabb.min.Y, 0)), (Vector3)(node.aabb.max));
                Handles.DrawLine((Vector3)(node.aabb.max), (Vector3)(new FixVec3(node.aabb.min.X, node.aabb.max.Y, 0)));
                Handles.DrawLine((Vector3)(new FixVec3(node.aabb.min.X, node.aabb.max.Y, 0)), (Vector3)(node.aabb.min));
            }
        }
#endif
    }
}