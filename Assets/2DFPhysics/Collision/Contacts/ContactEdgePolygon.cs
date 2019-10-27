using FixedPointy;
using TF.Colliders;

namespace TF.Core
{
    public class ContactEdgePolygon : ContactCallback
    {
        public static readonly ContactEdgePolygon instance = new ContactEdgePolygon();

        public void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b)
        {

        }
    }
}
