using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class CollisionEdgeEdge : CollisionCallback
    {
        public static readonly CollisionEdgeEdge instance = new CollisionEdgeEdge();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {

        }
    }
}