using FixedPointy;
using TF.Colliders;

namespace TF.Core
{
    public static class ContactChecks
    {
        public static ContactCallback[,] dispatch =
        new ContactCallback[3,3]
        {
            { ContactCircleCircle.instance, ContactCirclePolygon.instance, ContactCircleEdge.instance },
            { ContactPolygonCircle.instance, ContactPolygonPolygon.instance, ContactPolygonEdge.instance },
            { ContactEdgeCircle.instance, ContactEdgePolygon.instance, ContactEdgeEdge.instance }
        };

        public static bool AABBvsAABB(TFCollider aCol, TFCollider bCol)
        {
            // Exit with no intersection if found separated along an axis
            if (aCol.boundingBox.max.x < bCol.boundingBox.min.x || aCol.boundingBox.min.x > bCol.boundingBox.max.x) return false;

            if (aCol.boundingBox.max.y < bCol.boundingBox.min.y || aCol.boundingBox.min.y > bCol.boundingBox.max.y) return false;

            // No separating axis found, therefor there is at least one overlapping axis
            return true;
        }
    }
}
