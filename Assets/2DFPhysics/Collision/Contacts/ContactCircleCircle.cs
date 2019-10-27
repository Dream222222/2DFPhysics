using FixedPointy;
using TF.Colliders;

namespace TF.Core
{
    public class ContactCircleCircle : ContactCallback
    {
        public static readonly ContactCircleCircle instance = new ContactCircleCircle();

        public void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b)
        {
            TFCircleCollider A = (TFCircleCollider)a.coll;
            TFCircleCollider B = (TFCircleCollider)b.coll;

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