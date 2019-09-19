using FixedPointy;

namespace TDFP.Core.Shapes
{
    public struct Circle
    {
        public Fix radius;
        public FixVec2 position;

        public static Fix Distance(FixVec2 a, FixVec2 b)
        {
            return FixMath.Sqrt(FixMath.Pow((a.X - b.X), 2) + FixMath.Pow((a.Y - b.Y), 2));
        }

        public static bool CirclevsCircleOptimized(Circle a, Circle b)
        {
            Fix r = a.radius + b.radius;
            r *= r;
            return r < FixMath.Pow((a.position.X + b.position.X), 2) + FixMath.Pow((a.position.Y + b.position.Y), 2);
        }
    }
}
