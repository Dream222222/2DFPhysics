using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class ContactEdgePolygon : ContactCallback
    {
        public static readonly ContactEdgePolygon instance = new ContactEdgePolygon();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {

        }
    }
}
