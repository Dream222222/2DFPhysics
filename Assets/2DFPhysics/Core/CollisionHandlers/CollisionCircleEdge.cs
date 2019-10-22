using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class CollisionCircleEdge : CollisionCallback
    {
        public static readonly CollisionCircleEdge instance = new CollisionCircleEdge();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {
            CollisionEdgeCircle.instance.HandleCollision(m, b, a);

            if (m.contactCount > 0)
            {
                m.normal *= -Fix.One;
            }
        }
    }
}
