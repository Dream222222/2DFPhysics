using FixedPointy;
using TF.Colliders;

namespace TF.Core
{
    public class ContactEdgeEdge : ContactCallback
    {
        public static readonly ContactEdgeEdge instance = new ContactEdgeEdge();

        public void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b)
        {

        }
    }
}