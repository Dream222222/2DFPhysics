using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class CollisionEdgePolygon : CollisionCallback
    {
        public static readonly CollisionEdgePolygon instance = new CollisionEdgePolygon();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {

        }
    }
}
