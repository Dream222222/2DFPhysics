using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TDFP.Colliders;
using System;

namespace TDFP.Core
{
    public class FPRigidbody : MonoBehaviour
    {
        private TDFPTransform fpTransform;

        [SerializeField] protected RigidbodyType2D bodyType;
        public FPhysicsMaterial material;
        public bool simulated;
        public bool transformSmoothing;
        [SerializeField] public FixConst mass = 0;
        [SerializeField] public FixConst gravityScale = 0;
        public TDFPCollider coll;

        #region Varaibles
        public FPRInfo info;
        public AABB bounds;
        #endregion

        private void Awake()
        {
            fpTransform = GetComponent<TDFPTransform>();
            info.position = (FixVec2)fpTransform.position;
            info.velocity = new FixVec2(0, 0);
            info.angularVelocity = 0;
            info.torque = 0;
            info.force = FixVec2.Zero;
        }

        public void FPUpdate(Fix dt)
        {

        }

        public void UpdateTransform(float alpha)
        {
            Vector3 newPos = new Vector3((float)info.position.X, (float)info.position.Y, 0);
            if (transformSmoothing)
            {
                transform.position = (transform.position * alpha) + (newPos * (1.0f - alpha));
            }
            else
            {
                transform.position = newPos;
            }
        }

        public void AddForce(FixVec2 force, ForceMode2D mode = ForceMode2D.Force)
        {
            if(bodyType == RigidbodyType2D.Static)
            {
                return;
            }

            switch (mode) {
                case ForceMode2D.Force:
                    info.force += force;
                    break;
                case ForceMode2D.Impulse:
                    info.velocity += force;
                    break;
            }
        }

        public void ApplyImpulse(FixVec2 impulse, FixVec2 contactVector)
        {
            info.velocity += impulse * info.massData.inv_mass;
            info.angularVelocity += info.massData.inverse_inertia * FixVec2.Cross(contactVector, impulse);
        }

        public void SetBodyType(RigidbodyType2D rType)
        {
            bodyType = rType;
        }
    }
}