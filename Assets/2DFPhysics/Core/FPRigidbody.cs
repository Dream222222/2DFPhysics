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
        public FixVec2 Position { 
            get {
                return info.position;
            } 
            set
            {
                info.position = value;
                UpdateBounds((FixVec2)fpTransform.Position - value);
                fpTransform.Position = value;
            }
        }

        [HideInInspector] public Fix invMass;
        [HideInInspector] public Fix invInertia;

        public TDFPTransform fpTransform;

        [SerializeField] protected RigidbodyType2D bodyType;
        public FPhysicsMaterial material;
        public bool simulated;
        public bool transformSmoothing;
        [SerializeField] public FixConst mass = 1;
        [SerializeField] public FixConst inertia = 0;
        [SerializeField] public FixConst gravityScale = 0;
        public TFPCollider coll;

        public FPRInfo info;
        public AABB bounds;

        private void Awake()
        {
            fpTransform = GetComponent<TDFPTransform>();
            info.position = (FixVec2)fpTransform.Position;
            info.velocity = new FixVec2(0, 0);
            info.angularVelocity = 0;
            info.torque = 0;
            info.force = FixVec2.Zero;

            invMass = mass != Fix.Zero ? Fix.One / mass : Fix.Zero;
            invInertia = inertia != Fix.Zero ? Fix.One / inertia : Fix.Zero;

            RecalcAABB();
        }

        public bool dd;
        private void Update()
        {
            if (dd)
            {
                Debug.Log(info.velocity);
            }
        }

        public void UpdateBounds(FixVec2 diff)
        {
            coll.UpdateAABB(diff);
            bounds = coll.boundingBox;
        }

        public void RecalcAABB()
        {
            coll.RecalcAABB(info.position);
            bounds = coll.boundingBox;
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
            info.velocity += impulse * invMass;
            info.angularVelocity += invInertia * FixVec2.Cross(contactVector, impulse);
        }

        public void SetBodyType(RigidbodyType2D rType)
        {
            bodyType = rType;
        }

        public virtual void SetRotation(Fix rot)
        {
            info.rotation = rot;
            coll.SetRotation(rot);
        }
    }
}