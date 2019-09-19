using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDFP.Colliders
{
    public class TDFPCircleCollider : TDFPCollider
    {
        public FixConst radius;

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            UnityEditor.Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position + (new Vector3((float)offset.X, (float)offset.Y, 0)), Vector3.forward, ((float)radius));
        }
#endif
    }
}