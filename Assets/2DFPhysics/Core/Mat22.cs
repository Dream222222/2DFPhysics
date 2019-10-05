using FixedPointy;
using UnityEngine;

namespace TDFP.Core
{
    [System.Serializable]
    public class Mat22
    {
        public Fix m00, m01;
        public Fix m10, m11;

        public Mat22()
        {
            Set(0);
        }

        public Mat22(Fix radians)
        {
            Set(radians);
        }

        public Mat22(Fix a, Fix b, Fix c, Fix d)
        {
            Set(a, b, c, d);
        }

        public void Set(Fix radians)
        {
            Fix c = FixMath.Cos(radians);
            Fix s = FixMath.Sin(radians);

            m00 = c;
            m01 = -s;
            m10 = s;
            m11 = c;
        }

        public void Set(Fix a, Fix b, Fix c, Fix d)
        {
            m00 = a;
            m01 = b;
            m10 = c;
            m11 = d;
        }

        public void Set(Mat22 m)
        {
            m00 = m.m00;
            m01 = m.m01;
            m10 = m.m10;
            m11 = m.m11;
        }

        public Mat22 Abs()
        {
            Mat22 m = new Mat22();
            m.m00 = FixMath.Abs(m00);
            m.m01 = FixMath.Abs(m01);
            m.m10 = FixMath.Abs(m10);
            m.m11 = FixMath.Abs(m11);
            return m;
        }

        public FixVec2 GetAxisX()
        {
            return new FixVec2(m00, m10);
        }

        public FixVec2 GetAxisY()
        {
            return new FixVec2(m01, m11);
        }

        public void Transpose()
        {
            Fix t = m01;
            m01 = m10;
            m10 = t;
        }

        public Mat22 Transposed()
        {
            Mat22 m = new Mat22();
            m.m00 = m00;
            m.m01 = m10;
            m.m10 = m01;
            m.m11 = m11;
            return m;
        }

        public static Fix MatrixToRadian(Mat22 m)
        {
            return FixMath.Atan2(m.m10, m.m00);
        }

        public static Fix MatrixToDegrees(Mat22 m)
        {
            return MatrixToRadian(m) * 180 / FixMath.PI;
        }

        public static FixVec2 operator *(Mat22 lhs, FixVec2 rhs)
        {
            return new FixVec2(lhs.m00*rhs.X + lhs.m01*rhs.Y, lhs.m10*rhs.X + lhs.m11*rhs.Y);
        }

        public static Mat22 operator *(Mat22 lhs, Mat22 rhs)
        {
            Mat22 m = new Mat22();
            m.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10;
            m.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11;
            m.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10;
            m.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11;
            return m;
        }
    }
}