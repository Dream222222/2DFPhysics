using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TDFP.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDFP.Colliders
{
    public class TFPCircleCollider : TFPCollider
    {
        public Fix radius;

        public override void UpdateAABB(FixVec2 posDiff)
        {
            boundingBox.min._x += posDiff.X;
            boundingBox.min._y += posDiff.Y;
            boundingBox.max._x += posDiff.X;
            boundingBox.max._y += posDiff.Y;
        }

        public override void RecalcAABB(FixVec2 pos)
        {
            boundingBox.min._x = -radius + pos.X;
            boundingBox.min._y = -radius + pos.Y;
            boundingBox.max._x = radius + pos.X;
            boundingBox.max._y = radius + pos.Y;
        }

        public override TFPColliderType GetCType()
        {
            return TFPColliderType.Circle;
        }

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