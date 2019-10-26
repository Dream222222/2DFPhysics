using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
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

        public static bool AABBvsAABB(TFPCollider aCol, TFPCollider bCol)
        {
            // Exit with no intersection if found separated along an axis
            if (aCol.boundingBox.max._x < bCol.boundingBox.min._x || aCol.boundingBox.min._x > bCol.boundingBox.max._x) return false;

            if (aCol.boundingBox.max._y < bCol.boundingBox.min._y || aCol.boundingBox.min._y > bCol.boundingBox.max._y) return false;

            // No separating axis found, therefor there is at least one overlapping axis
            return true;
        }
    }
}
