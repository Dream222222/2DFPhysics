using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public static class CollisionChecks
    {
        public static bool AABBvsCircle(Manifold m)
        {
            TDFPCollider aCol = m.A.coll;
            TDFPCircleCollider bCol = (TDFPCircleCollider)m.B.coll;

            FixVec2 n = m.B.info.position - m.B.info.position;

            // Closest point on A to center of B
            FixVec2 closest = n;

            Fix aExtendX = (aCol.bounds.max.X - aCol.bounds.min.X) / (Fix.One + Fix.One);
            Fix aExtendY = (aCol.bounds.max.Y - aCol.bounds.min.Y) / (Fix.One + Fix.One);

            closest = new FixVec2(FixMath.Clamp(closest.X, -aExtendX, aExtendX), FixMath.Clamp(closest.Y, -aExtendY, aExtendY));

            bool inside = false;
            // Circle is inside the AABB, so we need to clamp the circle's center
            // to the closest edge
            if (n == closest)
            {
                inside = true;

                //Find closest axis
                if (FixMath.Abs(n.X) > FixMath.Abs(n.Y))
                {
                    if(closest.X > 0)
                    {
                        closest = new FixVec2(aExtendX, closest.Y);
                    }
                    else
                    {
                        closest = new FixVec2(-aExtendX, closest.Y);
                    }
                }
                else //Y axis is closer
                {
                    if (closest.Y > 0)
                    {
                        closest = new FixVec2(closest.X, aExtendY);
                    }
                    else
                    {
                        closest = new FixVec2(closest.X, -aExtendY);
                    }
                }
            }

            FixVec2 normal = n - closest;
            Fix d = normal.GetMagnitudeSquared();
            Fix r = bCol.radius;

            // Early out if the radius is shorter than distance to closest point and
            // Circle not inside the AABB
            if (d > r * r && !inside)
            {
                return false;
            }

            // Avoided sqrt until we needed
            d = normal.GetMagnitude();

            // Collision normal needs to be flipped to point outside if circle was
            // inside the AABB
            if (inside)
            {
                m.normal = -n;
                m.penetration = r - d;
            }
            else
            {
                m.normal = n;
                m.penetration = r - d;
            }

            return true;
        }

        public static bool AABBvsAABB(Manifold m)
        {
            TDFPCollider aCol = m.A.coll;
            TDFPCollider bCol = m.B.coll;

            FixVec2 n = m.B.info.position - m.B.info.position;

            Fix aExtendX = (aCol.bounds.max.X-aCol.bounds.min.X)/(Fix.One+Fix.One);
            Fix bExtendX = (bCol.bounds.max.X-bCol.bounds.min.X)/(Fix.One+Fix.One);

            Fix xOverlap = aExtendX + bExtendX - FixMath.Abs(n.X);

            // SAT test on x axis
            if (xOverlap > 0)
            {
                Fix aExtendY = (aCol.bounds.max.Y - aCol.bounds.min.Y) / (Fix.One+Fix.One);
                Fix bExtendY = (bCol.bounds.max.Y - bCol.bounds.min.Y) / (Fix.One + Fix.One);

                Fix yOverlap = aExtendY + bExtendY - FixMath.Abs(n.Y);

                // SAT test on y axis
                if (yOverlap > 0)
                {
                    // Find out which axis is axis of least penetration
                    if (xOverlap > yOverlap)
                    {
                        // Point towards B knowing that n points from A to B
                        if (n.X < 0)
                        {
                            m.normal = new FixVec2(-1, 0);
                        }
                        else
                        {
                            m.normal = new FixVec2(0, 0); //1,0 ?
                        }
                        m.penetration = xOverlap;
                        return true;
                    }
                    else
                    {
                        // Point toward B knowing that n points from A to B
                        if (n.Y < 0)
                        {
                            m.normal = new FixVec2(0, -1);
                        }
                        else
                        {
                            m.normal = new FixVec2(0, 1);
                        }
                        m.penetration = yOverlap;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CirclevsCircle(Manifold m)
        {
            TDFPCircleCollider aCol = (TDFPCircleCollider)m.A.coll;
            TDFPCircleCollider bCol = (TDFPCircleCollider)m.B.coll;

            FixVec2 n = m.B.info.position - m.A.info.position;

            Fix r = (Fix)aCol.radius + (Fix)bCol.radius;
            r *= r;

            if (n.GetMagnitudeSquared() > r)
            {
                return false;
            }

            // Circles have collided, now compute manifold
            Fix d = n.GetMagnitude(); // perform actual sqrt

            if(d != 0)
            {
                m.penetration = r - d;
                m.normal = n / d;
                return true;
            }
            else
            {
                m.penetration = aCol.radius;
                m.normal = new FixVec2(1, 0);
                return true;
            }
        }
    }
}
