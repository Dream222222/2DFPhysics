using FixedPointy;

namespace TDFP.Core{
    //Axis Aligned Bounding Box
    public struct AABB
    {
        public FixVec2 min;
        public FixVec2 max;

        public static AABB Union(AABB a, AABB b)
        {
            AABB C;
            C.min = new FixVec2(FixMath.Min(a.min.X, b.min.X), FixMath.Min(a.min.Y, b.min.Y));
            C.max = new FixVec2(FixMath.Min(a.max.X, b.max.X), FixMath.Min(a.max.Y, b.max.Y));
            return C;
        }

        public static Fix Area(AABB a)
        {
            FixVec2 d = a.max - a.min;
            return d.X * d.Y;
        }
    }
}