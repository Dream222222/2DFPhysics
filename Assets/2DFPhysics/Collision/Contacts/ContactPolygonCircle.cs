using FixedPointy;
using TF.Colliders;

namespace TF.Core
{
    public class ContactPolygonCircle : ContactCallback
    {
        public static readonly ContactPolygonCircle instance = new ContactPolygonCircle();

        public void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b)
        {
            ContactCirclePolygon.instance.HandleCollision(m, b, a);

            if(m.contactCount > 0)
            {
                m.normal *= -Fix.one;
            }
        }
    }
}
