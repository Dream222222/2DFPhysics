using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class CollisionPolygonCircle : CollisionCallback
    {
        public static readonly CollisionPolygonCircle instance = new CollisionPolygonCircle();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {
            CollisionCirclePolygon.instance.HandleCollision(m, b, a);

            if(m.contactCount > 0)
            {
                m.normal *= -Fix.One;
            }
        }
    }
}
