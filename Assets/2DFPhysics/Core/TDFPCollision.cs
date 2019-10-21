using FixedPointy;
using UnityEngine;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class TDFPCollision
    {
        public TFPCollider collider;
        public GameObject gameObject;
        public FPRigidbody rigidbody;
        public TDFPTransform transform;

        public TDFPCollision(TFPCollider collider)
        {
            this.collider = collider;
            gameObject = collider.gameObject;
            rigidbody = collider.body;
            transform = collider.tdTransform;
        }
    }
}
