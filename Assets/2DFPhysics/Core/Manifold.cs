using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using System;

namespace TDFP.Core
{
    [System.Serializable]
    public class Manifold
    {
        public FPRigidbody A;
        public FPRigidbody B;
        public Fix penetration;
        public FixVec2 normal;
        public FixVec2[] contacts = { new FixVec2(), new FixVec2() };
        public int contactCount;
        public Fix e;
        public Fix sf;
        public Fix df;

        public Manifold(FPRigidbody a, FPRigidbody b)
        {
            A = a;
            B = b;
        }

        public void solve()
        {
            int ia = (int)A.coll.GetCType();
            int ib = (int)B.coll.GetCType();

            CollisionChecks.dispatch[ia, ib].HandleCollision(this, A, B);
        }

        public void initialize()
        {
            // Calculate average bounciness/restitution
            e = FixMath.Min(A.material.bounciness, B.material.bounciness);

            // Calculate static & dynamic friction
            sf = FixMath.Sqrt(A.staticFriction * A.staticFriction + B.staticFriction * B.staticFriction);
            df = FixMath.Sqrt(A.dynamicFriction * A.dynamicFriction + B.dynamicFriction * B.dynamicFriction);

            for(int i = 0; i < contactCount; i++)
            {
                //Calculate radii from COM to contact
                FixVec2 ra = contacts[i] -= A.info.position;
                FixVec2 rb = contacts[i] -= B.info.position;

                //?
                FixVec2 rv = B.info.velocity + FixVec2.Cross(B.info.angularVelocity, rb)
                    - A.info.velocity - FixVec2.Cross(A.info.angularVelocity, ra);

                // Determine if we should perform a resting collision or not
                // The idea is if the only thing moving this object is gravity,
                // then the collision should be performed without any restitution
                if(rv.GetMagnitudeSquared() < TDFPhysics.instance.resting)
                {
                    e = 0;
                }
            }
        }

        public void ApplyImpulse()
        {
            // Early out and positional correct if both objects have infinite mass
            if(A.invMass + B.invMass == 0)
            {
                InfiniteMassCorrection();
                return;
            }

            for(int i = 0; i < contactCount; ++i)
            {
                // Calculate radii from COM to contact
                FixVec2 ra = contacts[i] - A.Position;
                FixVec2 rb = contacts[i] - B.Position;

                //Relative velocity
                FixVec2 rv = B.info.velocity + FixVec2.Cross(B.info.angularVelocity, rb) - A.info.velocity - FixVec2.Cross(A.info.angularVelocity, ra);

                //Relative velocity along the normal
                Fix contactVel = rv.Dot(normal);

                if(contactVel > 0)
                {
                    return;
                }

                Fix raCrossN = FixVec2.Cross(ra, normal);
                Fix rbCrossN = FixVec2.Cross(rb, normal);
                Fix invMassSum = A.invMass + B.invMass 
                    + (raCrossN * raCrossN) * A.invInertia
                    + (rbCrossN * rbCrossN) * B.invInertia;

                // Calculate impulse scalar
                Fix j = -(Fix.One + e) * contactVel;
                j /= invMassSum;
                j /= contactCount;

                // Apply impulse
                FixVec2 impulse = normal * j;
                A.ApplyImpulse(-impulse, ra);
                B.ApplyImpulse(impulse, rb);

                // Friction Impulse
                rv = B.info.velocity + FixVec2.Cross(B.info.angularVelocity, rb)
                    - A.info.velocity - FixVec2.Cross(A.info.angularVelocity, ra);

                FixVec2 t = rv - (normal * FixVec2.Dot(rv, normal));
                t.Normalize();

                // j tangent magnitude
                Fix jt = -FixVec2.Dot(rv, t);
                jt /= invMassSum;
                jt /= contactCount;

                //Don't apply tiny friction impulses
                if(FixMath.Abs(jt) <= Fix.Zero)
                {
                    return;
                }

                // Coulumb's law
                FixVec2 tangentImpulse;
                if (FixMath.Abs(jt) < j * sf)
                {
                    tangentImpulse = t * jt;
                }
                else
                {
                    tangentImpulse = t * -j * df;
                }

                // Apply friction impulse
                A.ApplyImpulse(-tangentImpulse, ra);
                B.ApplyImpulse(tangentImpulse, rb);
            }
        }

        public void PositionalCorrection()
        {
            TDFPhysics settings = TDFPhysics.instance;
            FixVec2 correction = (FixMath.Max(penetration - settings.penetrationAllowance, Fix.Zero)) / (A.invMass + B.invMass) 
                * normal * settings.penetrationCorrection;

            A.Position -= correction * A.invMass;
            B.Position += correction * B.invMass;
        }

        private void InfiniteMassCorrection()
        {
            A.info.velocity = FixVec2.Zero;
            B.info.velocity = FixVec2.Zero;
        }
    }
}