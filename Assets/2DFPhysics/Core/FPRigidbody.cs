using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TDFP.Colliders;
using System;
using Unity.Collections;

namespace TDFP.Core
{
    [ExecuteInEditMode]
    public class FPRigidbody : MonoBehaviour
    {
        public FixVec2 Position { 
            get {
                return info.position;
            } 
            set
            {
                info.position = value;
                UpdateBounds(value - (FixVec2)fpTransform.Position);
                fpTransform.Position = value;
            }
        }

        [HideInInspector] public Fix invMass;
        [HideInInspector] public Fix invInertia;

        [SerializeField] protected RigidbodyType2D bodyType;

        #region References
        public TDFPTransform fpTransform;
        public TFPCollider coll;
        public FPhysicsMaterial material;
        #endregion

        public bool simulated;
        public bool transformSmoothing;
        [SerializeField] public Fix mass;
        [SerializeField] public Fix inertia;
        [SerializeField] public Fix gravityScale;
        [SerializeField] public Fix staticFriction;
        [SerializeField] public Fix dynamicFriction;
        [ReadOnly]public FPRInfo info;
        public AABB bounds;

        private void Awake()
        {
            fpTransform = GetComponent<TDFPTransform>();
            if (!Application.isPlaying)
            {
                //In edit mode, return out.
                return;
            }
            info.position = (FixVec2)fpTransform.Position;
            info.velocity = new FixVec2(0, 0);
            info.angularVelocity = 0;
            info.torque = 0;
            info.force = FixVec2.Zero;

            invMass = mass != Fix.Zero ? Fix.One / mass : Fix.Zero;
            invInertia = inertia != Fix.Zero ? Fix.One / inertia : Fix.Zero;
        }

        private void Start()
        {
            RecalcAABB();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                //In edit mode.
                if (transform.hasChanged)
                {
                    info.position = (FixVec2)fpTransform.Position;
                    info.rotation = Mat22.MatrixToRadian(fpTransform.rotation);
                }
            }
        }

        // Update our AABB with the difference in position.
        public void UpdateBounds(FixVec2 diff)
        {
            coll.UpdateAABB(diff);
            bounds = coll.boundingBox;
        }

        // Recalculate our AABB. This happens mainly whenever we rotate.
        public void RecalcAABB()
        {
            coll.RecalcAABB(info.position);
            bounds = coll.boundingBox;
        }

        public void FPUpdate(Fix dt)
        {

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
            info.velocity += impulse * invMass;
            info.angularVelocity += invInertia * FixVec2.Cross(contactVector, impulse);
        }

        public void SetBodyType(RigidbodyType2D rType)
        {
            bodyType = rType;
        }

        public virtual void SetRotation(Fix radians)
        {
            info.rotation = radians;
            coll.SetRotation(radians);
            fpTransform.Rotation = coll.u;
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
    }
}