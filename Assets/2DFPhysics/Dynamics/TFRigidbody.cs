using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TF.Colliders;
using System;
using Unity.Collections;

namespace TF.Core
{
    [ExecuteInEditMode]
    public class TFRigidbody : MonoBehaviour
    {
        #region Collision Events
        public delegate void OnCollisionEnterAction(TFCollision collision);
        public event OnCollisionEnterAction OnCollisionEnter;
        public delegate void OnCollisionStayAction(TFCollision collision);
        public event OnCollisionStayAction OnCollisionStay;
        public delegate void OnCollisionEndAction(TFCollision collision);
        public event OnCollisionEndAction OnCollisionExit;
        public delegate void OnTriggerEnterAction(TFCollider coll);
        public event OnTriggerEnterAction OnTriggerEnter;
        public delegate void OnTriggerStayAction(TFCollider coll);
        public event OnTriggerStayAction OnTriggerStay;
        public delegate void OnTriggerEndAction(TFCollider coll);
        public event OnTriggerEndAction OnTriggerExit;
        #endregion

        [HideInInspector] public List<TFCollider> lastCollidedWith = new List<TFCollider>();
        [HideInInspector] public List<TFCollider> currentlyCollidingWith = new List<TFCollider>();

        public int ProxyID { get; set; } = -1;

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
        public TFTransform fpTransform;
        public TFCollider coll;
        public TFPhysicsMaterial material;
        #endregion

        public bool simulated = true;
        public TFBodyType bodyType;
        public InterpolationType interpolation;
        [SerializeField] public Fix mass = 1;
        [SerializeField] public Fix inertia = 0;
        [SerializeField] public Fix gravityScale = 1;
        [ReadOnly]public FPRInfo info;
        public AABB bounds;

        private void Awake()
        {
            fpTransform = GetComponent<TFTransform>();
            coll = GetComponent<TFCollider>();
        }

        private void Start()
        {
            fpTransform = GetComponent<TFTransform>();
            coll = GetComponent<TFCollider>();
            if (!Application.isPlaying)
            {
                //In edit mode, return out.
                return;
            }
            coll.body = this;
            info.position = (FixVec2)fpTransform.Position;
            info.velocity = new FixVec2(0, 0);
            info.angularVelocity = 0;
            info.torque = 0;
            info.force = FixVec2.Zero;

            invMass = mass != Fix.Zero ? Fix.One / mass : Fix.Zero;
            invInertia = inertia != Fix.Zero ? Fix.One / inertia : Fix.Zero;
            RecalcAABB();
            TFPhysics.AddBody(this);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                //In edit mode, return out.
                return;
            }
            TFPhysics.RemoveBody(this);
        }

        private void Update()
        {
            // EDIT MODE ONLY
            if (!Application.isPlaying)
            {
                if (transform.hasChanged)
                {
                    info.position = (FixVec2)fpTransform.Position;
                    info.rotation = Mat22.MatrixToDegrees(fpTransform.rotation);
                    SetRotation(info.rotation);
                    RecalcAABB();
                }
            }
        }

        public void FPUpdate(Fix dt)
        {

        }

        #region AABB
        // Update our AABB with the difference in position.
        public void UpdateBounds(FixVec2 diff)
        {
            coll.MoveAABB(diff);
            bounds = coll.boundingBox;
        }

        // Recalculate our AABB. This has to happen whenever we rotate.
        public void RecalcAABB()
        {
            coll.RecalcAABB(info.position);
            bounds = coll.boundingBox;
        }
        #endregion

        #region Forces
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

        public virtual void SetRotation(Fix degrees)
        {
            info.rotation = degrees;
            coll.SetRotation(degrees);
            fpTransform.Rotation = coll.u;
        }
        #endregion

        #region Events
        public void HandlePhysicsEvents()
        {
            for (int i = 0; i < currentlyCollidingWith.Count; i++)
            {
                if (lastCollidedWith.Contains(currentlyCollidingWith[i]))
                {
                    //Collided with it last frame.
                    if (coll.isTrigger)
                    {
                        OnTriggerStay?.Invoke(currentlyCollidingWith[i]);
                    }
                    else
                    {
                        OnCollisionStay?.Invoke(new TFCollision(currentlyCollidingWith[i]));
                    }
                }
                else
                {
                    //Did not collide with last frame.
                    if (coll.isTrigger)
                    {
                        OnTriggerEnter?.Invoke(currentlyCollidingWith[i]);
                    }
                    else
                    {
                        OnCollisionEnter?.Invoke(new TFCollision(currentlyCollidingWith[i]));
                    }
                }
            }

            for (int w = 0; w < lastCollidedWith.Count; w++)
            {
                //If we've exited collision with a collider.
                if (!currentlyCollidingWith.Contains(lastCollidedWith[w]))
                {
                    if (coll.isTrigger)
                    {
                        OnTriggerExit?.Invoke(lastCollidedWith[w]);
                    }
                    else
                    {
                        OnCollisionExit?.Invoke(new TFCollision(lastCollidedWith[w]));
                    }
                }
            }
            lastCollidedWith = new List<TFCollider>(currentlyCollidingWith);
            currentlyCollidingWith.Clear();
        }
        #endregion
    }
}