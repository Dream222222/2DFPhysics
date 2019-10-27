using FixedPointy;
using TF.Colliders;

namespace TF.Core
{
    public class ContactPolygonEdge : ContactCallback
    {
        public static readonly ContactPolygonEdge instance = new ContactPolygonEdge();

        public void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b)
        {

        }
    }
}