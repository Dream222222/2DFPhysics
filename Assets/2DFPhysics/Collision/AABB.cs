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
            return (aabb.min.X >= min.X && aabb.min.Y >= min.Y
                && aabb.max.X <= max.X && aabb.max.Y <= max.Y);
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

            if (d1._x > Fix.Zero || d1._y > Fix.Zero)
                return false;

            if (d2._x > Fix.Zero || d2._y > Fix.Zero)
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
            Fix wx = a.max._x - a.min._x;
            Fix wy = a.max._y - a.min._y;
            return 2 * (wx + wy);
        }
    }
}