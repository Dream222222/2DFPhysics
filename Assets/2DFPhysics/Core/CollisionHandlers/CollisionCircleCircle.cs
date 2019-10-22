using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class CollisionCircleCircle : CollisionCallback
    {
        public static readonly CollisionCircleCircle instance = new CollisionCircleCircle();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {
            TFPCircleCollider A = (TFPCircleCollider)a.coll;
            TFPCircleCollider B = (TFPCircleCollider)b.coll;

            //Calculate translational vector, which is normal
            FixVec2 normal = ((FixVec2)b.Position) - ((FixVec2)a.Position);

            Fix distSpr = normal.GetMagnitudeSquared();
            Fix radius = A.radius + B.radius;

            //Not in contact
            if(distSpr >= radius * radius)
            {
                m.contactCount = 0;
                return;
            }

            Fix distance = FixMath.Sqrt(distSpr);

            m.contactCount = 1;

            if(distance == Fix.Zero)
            {
                m.penetration = A.radius;
                m.normal = new FixVec2(1, 0);
                m.contacts[0] = (FixVec2)a.Position;
            }
            else
            {
                m.penetration = radius - distance;
                m.normal = normal / distance;
                m.contacts[0] = m.normal * A.radius + ((FixVec2)a.Position);
            }
        }
    }
}