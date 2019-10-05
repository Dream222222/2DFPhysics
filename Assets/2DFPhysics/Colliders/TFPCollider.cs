using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TDFP.Core;

namespace TDFP.Colliders
{
    public class TFPCollider : MonoBehaviour
    {
        public FPRigidbody body;
        public TDFPTransform tdTransform;
        public AABB boundingBox;
        public bool isTrigger;
        public FixVec2 offset;
        public Mat22 u = new Mat22(0);

        protected virtual void Awake()
        {
            RecalcAABB((FixVec2)tdTransform.Position);
        }

        public virtual void UpdateAABB(FixVec2 pos)
        {

        }

        public virtual void RecalcAABB(FixVec2 pos)
        {

        }

        public virtual void SetRotation(Fix radians)
        {
            u.Set(radians);
        }

        public virtual TFPColliderType GetCType()
        {
            return 0;
        }
    }
}