using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class ContactEdgeEdge : ContactCallback
    {
        public static readonly ContactEdgeEdge instance = new ContactEdgeEdge();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {

        }
    }
}