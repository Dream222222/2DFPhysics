using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public static class CollisionChecks
    {
        public static CollisionCallback[,] dispatch =
        new CollisionCallback[3,3]
        {
            { CollisionCircleCircle.instance, CollisionCirclePolygon.instance, CollisionCircleEdge.instance },
            { CollisionPolygonCircle.instance, CollisionPolygonPolygon.instance, CollisionPolygonEdge.instance },
            { CollisionEdgeCircle.instance, CollisionEdgePolygon.instance, CollisionEdgeEdge.instance }
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
