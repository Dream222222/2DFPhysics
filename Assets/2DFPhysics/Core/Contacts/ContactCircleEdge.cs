using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class ContactCircleEdge : ContactCallback
    {
        public static readonly ContactCircleEdge instance = new ContactCircleEdge();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {
            ContactEdgeCircle.instance.HandleCollision(m, b, a);

            if (m.contactCount > 0)
            {
                m.normal *= -Fix.One;
            }
        }
    }
}
