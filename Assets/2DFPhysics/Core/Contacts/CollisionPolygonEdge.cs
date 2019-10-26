using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class CollisionPolygonEdge : CollisionCallback
    {
        public static readonly CollisionPolygonEdge instance = new CollisionPolygonEdge();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {

        }
    }
}