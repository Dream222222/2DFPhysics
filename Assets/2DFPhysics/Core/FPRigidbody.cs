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
        public delegate void OnTriggerEnterAction(TFPCollider coll);
        public event OnTriggerEnterAction OnTriggerEnter;
        public delegate void OnTriggerStayAction(TFPCollider coll);
        public event OnTriggerStayAction OnTriggerStay;
        public delegate void OnTriggerEndAction(TFPCollider coll);
        public event OnTriggerEndAction OnTriggerExit;

        [HideInInspector] public List<TFPCollider> lastCollidedWith = new List<TFPCollider>();
        [HideInInspector] public List<TFPCollider> currentlyCollidingWith = new List<TFPCollider>();

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
        public Fix StaticFriction {
            get
            {
                if (!material)
                {
                    return (Fix)0.4f;
                }
                return material.staticFriction;
            }
        }
        public Fix DynamicFriction
        {
            get
            {
                if (!material)
                {
                    return (Fix)0.2f;
                }
                return material.dynamicFriction;
            }
        }
        public Fix Bounciness
        {
            get
            {
                if (!material)
                {
                    return (Fix)0;
                }
                return material.bounciness;
            }
        }


        [HideInInspector] public Fix invMass;
        [HideInInspector] public Fix invInertia;

        #region References
        public TDFPTransform fpTransform;
        public TFPCollider coll;
        public FPhysicsMaterial material;
        #endregion

        public bool simulated = true;
        public InterpolationType interpolation;
        [SerializeField] public Fix mass = 1;
        [SerializeField] public Fix inertia = 0;
        [SerializeField] public Fix gravityScale = 1;
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
            TDFPhysics.bodies.Add(this);
            coll = GetComponent<TFPCollider>();
            coll.body = this;
            info.position = (FixVec2)fpTransform.Position;
            info.velocity = new FixVec2(0, 0);
            info.angularVelocity = 0;
            info.torque = 0;
            info.force = FixVec2.Zero;

            invMass = mass != Fix.Zero ? Fix.One / mass : Fix.Zero;
            invInertia = inertia != Fix.Zero ? Fix.One / inertia : Fix.Zero;
        }

        private void OnDestroy()
        {
            TDFPhysics.bodies.Remove(this);
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
            if(mass == 0)
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

        public virtual void SetRotation(Fix radians)
        {
            info.rotation = radians;
            coll.SetRotation(radians);
            fpTransform.Rotation = coll.u;
        }

        public void UpdateTransform(float alpha)
        {
            /*
            Vector3 newPos = new Vector3((float)info.position.X, (float)info.position.Y, 0);
            if (transformSmoothing)
            {
                transform.position = (transform.position * alpha) + (newPos * (1.0f - alpha));
            }
            else
            {
                transform.position = newPos;
            }*/
        }

        public void HandlePhysicsEvents()
        {
            for(int i = 0; i < currentlyCollidingWith.Count; i++)
            {
                if (lastCollidedWith.Contains(currentlyCollidingWith[i]))
                {
                    //Collided with it last frame.
                    if (coll.isTrigger)
                    {
                        OnTriggerStay?.Invoke(currentlyCollidingWith[i]);
                    }
                }
                else
                {
                    //Did not collide with last frame.
                    if (coll.isTrigger)
                    {
                        OnTriggerEnter?.Invoke(currentlyCollidingWith[i]);
                    }
                }
            }

            for(int w = 0; w < lastCollidedWith.Count; w++)
            {
                //If we've exited collision with a collider.
                if (!currentlyCollidingWith.Contains(lastCollidedWith[w]))
                {
                    if (coll.isTrigger)
                    {
                        OnTriggerExit?.Invoke(lastCollidedWith[w]);
                    }
                }
            }
            lastCollidedWith = new List<TFPCollider>(currentlyCollidingWith);
            currentlyCollidingWith.Clear();
        }
    }
}