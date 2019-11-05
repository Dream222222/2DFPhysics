using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TF.Core;

namespace TF.Colliders
{
    public class TFCollider : MonoBehaviour
    {
        public FixVec2 Size
        {
            get
            {
                return boundingBox.max - boundingBox.min;
            }
        }

        public TFRigidbody body;
        public TFTransform tdTransform;
        public AABB boundingBox;
        public bool isTrigger;
        public FixVec2 offset;
        public Mat22 u = new Mat22(0);

        protected virtual void Awake()
        {
            body = GetComponent<TFRigidbody>();
            tdTransform = GetComponent<TFTransform>();
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

        public virtual TFColliderType GetCType()
        {
            return 0;
        }

        public virtual bool Raycast(out TFRaycastHit2D hit, FixVec2 pointA, FixVec2 pointB, Fix maxFraction)
        {
            hit = default;
            return false;
        }
    }
}