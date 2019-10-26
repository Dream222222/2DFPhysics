using FixedPointy;
using TDFP.Colliders;
using UnityEngine;

namespace TDFP.Core
{
    public class CollisionCirclePolygon : CollisionCallback
    {
        public static readonly CollisionCirclePolygon instance = new CollisionCirclePolygon();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {
            TFPCircleCollider A = (TFPCircleCollider)a.coll;
            TFPPolygonCollider B = (TFPPolygonCollider)b.coll;

            m.contactCount = 0;

            // Transform circle center to Polygon model space
            FixVec2 center = B.u.Transposed() * (a.Position - b.Position);

            // Find edge with minimum penetration
            // Exact concept as using support points in Polygon vs Polygon
            Fix separation = -Fix.MaxValue;
            int faceNormal = 0;
            for(int i = 0; i < B.vertices.Count; ++i)
            {
                Fix s = FixVec2.Dot(B.normals[i], center - B.vertices[i]);

                if(s > A.radius)
                {
                    return;
                }

                if(s > separation)
                {
                    separation = s;
                    faceNormal = i;
                }
            }

            // Grab face's vertices
            FixVec2 v1 = B.vertices[faceNormal];
            int i2 = (faceNormal + 1) < B.vertices.Count ? faceNormal + 1 : 0;
            FixVec2 v2 = B.vertices[i2];

            // Check to see if center is within polygon
            if (separation < Fix.Epsilon)
            {
                m.contactCount = 1;
                m.normal = -(B.u * B.normals[faceNormal]);
                m.contacts[0] = m.normal * A.radius + a.Position;
                m.penetration = A.radius;
                return;
            }

            // Determine which voronoi region of the edge center of circle lies within
            Fix dot1 = FixVec2.Dot(center - v1, v2 - v1);
            Fix dot2 = FixVec2.Dot(center - v2, v1 - v2);
            m.penetration = A.radius - separation;

            //Closest to v1
            if(dot1 <= Fix.Zero)
            {
                if ((center-v1).GetMagnitudeSquared() > A.radius * A.radius)
                {
                    return;
                }

                m.contactCount = 1;
                FixVec2 n = v1 - center;
                n = B.u * n;
                n = n.Normalized();
                m.normal = n;
                v1 = B.u * v1 + b.Position;
                m.contacts[0] = v1;
            }
            else if(dot2 <= Fix.Zero)
            {
                //Closest to v2
                if ((center-v2).GetMagnitudeSquared() > A.radius * A.radius)
                {
                    return;
                }

                m.contactCount = 1;
                FixVec2 n = v2 - center;
                v2 = B.u * v2 + b.Position;
                m.contacts[0] = v2;
                n = B.u * n;
                n = n.Normalized();
                m.normal = n;
            }
            else
            {
                //Closest to face
                FixVec2 n = B.normals[faceNormal];
                if (FixVec2.Dot(center-v1, n) > A.radius)
                {
                    return;
                }

                n = B.u * n;
                m.normal = -n;
                m.contacts[0] = m.normal * A.radius + a.Position;
                m.contactCount = 1;
            }
        }
    }
}
