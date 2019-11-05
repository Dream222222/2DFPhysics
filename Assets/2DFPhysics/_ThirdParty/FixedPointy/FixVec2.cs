/* FixedPointy - A simple fixed-point math library for C#.
 * 
 * Copyright (c) 2013 Jameson Ernst
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using UnityEngine;

namespace FixedPointy {
    [Serializable]
	public struct FixVec2 {
		public static readonly FixVec2 zero = new FixVec2();
		public static readonly FixVec2 one = new FixVec2(1, 1);
        public static readonly FixVec2 right = new FixVec2(1, 0);
        public static readonly FixVec2 up = new FixVec2(0, 1);

        public static bool operator ==(FixVec2 lhs, FixVec2 rhs)
        {
            return lhs.X.raw == rhs.X.raw
                && lhs.Y.raw == rhs.Y.raw;
        }

        public static FixVec2 Abs(FixVec2 v)
        {
            return new FixVec2(FixMath.Abs(v.x), FixMath.Abs(v.y));
        }

        public static bool operator !=(FixVec2 lhs, FixVec2 rhs)
        {
            return lhs.X.raw != rhs.X.raw
                || lhs.Y.raw != rhs.Y.raw;
        }

        internal static FixVec2 Min(FixVec2 pointA, FixVec2 t)
        {
            return new FixVec2(FixMath.Min(pointA.x, t.x), FixMath.Min(pointA.y, t.y));
        }

        internal static FixVec2 Max(FixVec2 pointA, FixVec2 t)
        {
            return new FixVec2(FixMath.Max(pointA.x, t.x), FixMath.Max(pointA.y, t.y));
        }

        public static FixVec2 operator + (FixVec2 rhs) {
			return rhs;
		}
		public static FixVec2 operator - (FixVec2 rhs) {
			return new FixVec2(-rhs.x, -rhs.y);
		}

		public static FixVec2 operator + (FixVec2 lhs, FixVec2 rhs) {
			return new FixVec2(lhs.x + rhs.x, lhs.y + rhs.y);
		}
		public static FixVec2 operator - (FixVec2 lhs, FixVec2 rhs) {
			return new FixVec2(lhs.x - rhs.x, lhs.y - rhs.y);
		}

		public static FixVec2 operator + (FixVec2 lhs, Fix rhs) {
			return lhs.ScalarAdd(rhs);
		}
		public static FixVec2 operator + (Fix lhs, FixVec2 rhs) {
			return rhs.ScalarAdd(lhs);
		}
		public static FixVec2 operator - (FixVec2 lhs, Fix rhs) {
			return new FixVec2(lhs.x - rhs, lhs.y - rhs);
		}

        public static FixVec2 operator *(FixVec2 lhs, FixVec2 rhs)
        {
            return new FixVec2(lhs.x * rhs.x, lhs.y * rhs.y);
        }

        public static FixVec2 operator *(FixVec2 lhs, FixVec3 rhs)
        {
            return new FixVec2(lhs.x * rhs.x, lhs.y * rhs.y);
        }

        public static FixVec2 operator * (FixVec2 lhs, Fix rhs) {
			return lhs.ScalarMultiply(rhs);
		}

		public static FixVec2 operator * (Fix lhs, FixVec2 rhs) {
			return rhs.ScalarMultiply(lhs);
		}

		public static FixVec2 operator / (FixVec2 lhs, Fix rhs) {
			return new FixVec2(lhs.x / rhs, lhs.y / rhs);
		}

        public static explicit operator FixVec2(FixVec3 v)
        {
            return new FixVec2(v.X, v.Y);
        }

        public static Fix Dot(FixVec2 lhs, FixVec2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public static Fix Cross(FixVec2 lhs, FixVec2 rhs)
        {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }

        public static FixVec2 Cross(Fix lhs, FixVec2 rhs)
        {
            return new FixVec2(rhs.Y * -lhs, rhs.X * lhs);
        }

        public static FixVec2 Cross(FixVec2 lhs, Fix rhs)
        {
            return new FixVec2(lhs.Y * rhs, lhs.X * -rhs);
        }

        public Fix x, y;

		public FixVec2 (Fix x, Fix y) {
			this.x = x;
			this.y = y;
		}

		public Fix X { get { return x; } }
		public Fix Y { get { return y; } }

		public Fix Dot (FixVec2 rhs) {
			return x * rhs.x + y * rhs.y;
		}

		public Fix Cross (FixVec2 rhs) {
			return x * rhs.y - y * rhs.x;
		}

		FixVec2 ScalarAdd (Fix value) {
			return new FixVec2(x + value, y + value);
		}
		FixVec2 ScalarMultiply (Fix value) {
			return new FixVec2(x * value, y * value);
		}

		public Fix GetMagnitude () {
			ulong N = (ulong)((long)x.raw * (long)x.raw + (long)y.raw * (long)y.raw);

			return new Fix((int)(FixMath.SqrtULong(N << 2) + 1) >> 1);
		}

        public Fix GetMagnitudeSquared()
        {
            return (x*x)+(y*y);
        }

		public void Normalize () {
            if (x == 0 && y == 0)
            {
                return;
            }

			Fix m = GetMagnitude();
            x = x / m;
            y = y / m;
		}

        public FixVec2 Normalized()
        {
            if (x == 0 && y == 0)
                return FixVec2.zero;

            var m = GetMagnitude();
            return new FixVec2(x / m, y / m);
        }

        public override string ToString () {
			return string.Format("({0}, {1})", x, y);
		}

        public static explicit operator Vector3(FixVec2 v)
        {
            return new Vector3((float)v.X, (float)v.Y, 0);
        }

        public override bool Equals(System.Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                FixVec2 fv = (FixVec2)obj;
                return x == fv.x.raw && y.raw == fv.y.raw;
            }
        }

        public override int GetHashCode()
        {
            return x.raw.GetHashCode() ^ y.raw.GetHashCode() << 2;
        }
    }
}
