using FixedPointy;

namespace TF.Core{
    //Axis Aligned Bounding Box
    public struct AABB
    {
        public FixVec2 min;
        public FixVec2 max;

        /// <summary>
        /// Returns true if this AABB contains the other.
        /// </summary>
        /// <param name="aabb"></param>
        /// <returns></returns>
        public bool Contains(AABB aabb)
        {
            return min.x <= aabb.min.x
                && min.y <= aabb.min.y
                && aabb.max.x <= max.x
                && aabb.max.y <= max.y;
        }

        /// <summary>
        /// Returns true if the AABBs overlap.
        /// </summary>
        /// <param name="aabb"></param>
        /// <returns></returns>
        public bool Overlaps(AABB aabb)
        {
            FixVec2 d1, d2;
            d1 = aabb.min - max;
            d2 = min - aabb.max;

            if (d1.x > Fix.Zero || d1.y > Fix.Zero)
                return false;

            if (d2.x > Fix.Zero || d2.y > Fix.Zero)
                return false;

            return true;
        }

        public Fix Area()
        {
            return Area(this);
        }

        public static AABB Union(AABB a, AABB b)
        {
            AABB C;
            C.min = new FixVec2(FixMath.Min(a.min.X, b.min.X), FixMath.Min(a.min.Y, b.min.Y));
            C.max = new FixVec2(FixMath.Max(a.max.X, b.max.X), FixMath.Max(a.max.Y, b.max.Y));
            return C;
        }

        public static Fix Area(AABB a)
        {
            Fix wx = a.max.x - a.min.x;
            Fix wy = a.max.y - a.min.y;
            return 2 * (wx + wy);
        }
    }
}