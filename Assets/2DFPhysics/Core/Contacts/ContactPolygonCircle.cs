using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class ContactPolygonCircle : ContactCallback
    {
        public static readonly ContactPolygonCircle instance = new ContactPolygonCircle();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {
            ContactCirclePolygon.instance.HandleCollision(m, b, a);

            if(m.contactCount > 0)
            {
                m.normal *= -Fix.One;
            }
        }
    }
}
