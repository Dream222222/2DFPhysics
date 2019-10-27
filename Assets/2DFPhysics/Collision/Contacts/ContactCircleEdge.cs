using FixedPointy;
using TF.Colliders;

namespace TF.Core
{
    public class ContactCircleEdge : ContactCallback
    {
        public static readonly ContactCircleEdge instance = new ContactCircleEdge();

        public void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b)
        {
            ContactEdgeCircle.instance.HandleCollision(m, b, a);

            if (m.contactCount > 0)
            {
                m.normal *= -Fix.One;
            }
        }
    }
}
