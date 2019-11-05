using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TF.Core
{
    public class TFPhysics : MonoBehaviour
    {
        public static TFPhysics instance;
        public static TFPhysicsScene physicsScene = new TFPhysicsScene();

        [HideInInspector] public Fix resting;
        [HideInInspector] public Fix penetrationAllowance = (Fix)0.05f;
        [HideInInspector] public Fix penetrationCorrection = (Fix)0.4f;

        public TFSettings settings;

        public bool debug;

        private void Awake()
        {
            instance = this;
            resting = (settings.gravity * settings.deltaTime).GetMagnitudeSquared() + Fix.Epsilon;
        }

        private void FixedUpdate()
        { 
            if (settings.AutoSimulation)
            {
                StepPhysics(settings.deltaTime);
            }
        }

        public void StepPhysics(Fix dt)
        {
            physicsScene.Step();
        }

        /// <summary>
        /// Adds a body to the simulation.
        /// </summary>
        /// <param name="body">The rigidbody to add.</param>
        public static void AddBody(TFRigidbody body)
        {
            physicsScene.AddBody(body, (Fix)0.2f);
        }

        /// <summary>
        /// Removes a body from the simulation.
        /// </summary>
        /// <param name="body">The rigidbody to remove.</param>
        public static void RemoveBody(TFRigidbody body)
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
        public bool Raycast(FixVec2 pointA, FixVec2 pointB)
        {
            TFRaycastHit2D hit;
            return physicsScene.Raycast(pointA, pointB, out hit);
        }
        #endregion

        #region Editor
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
        #endregion
    }
}