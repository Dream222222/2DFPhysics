using FixedPointy;
using UnityEngine;
using TF.Colliders;

namespace TF.Core
{
    public class TFCollision
    {
        public TFCollider collider;
        public GameObject gameObject;
        public TFRigidbody rigidbody;
        public TFTransform transform;

        public TFCollision(TFCollider collider)
        {
            this.collider = collider;
            gameObject = collider.gameObject;
            rigidbody = collider.body;
            transform = collider.tdTransform;
        }
    }
}
