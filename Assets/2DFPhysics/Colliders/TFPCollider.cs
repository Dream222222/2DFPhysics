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
            body = GetComponent<FPRigidbody>();
            tdTransform = GetComponent<TDFPTransform>();
            RecalcAABB((FixVec2)tdTransform.Position);
        }

        public virtual void MoveAABB(FixVec2 posDiff)
        {

        }

        public virtual void RecalcAABB(FixVec2 pos)
        {

        }

        public virtual void SetRotation(Fix degrees)
        {
            u.Set(degrees);
        }

        public virtual TFPColliderType GetCType()
        {
            return 0;
        }
    }
}