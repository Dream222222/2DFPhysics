using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class ContactPolygonEdge : ContactCallback
    {
        public static readonly ContactPolygonEdge instance = new ContactPolygonEdge();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {

        }
    }
}