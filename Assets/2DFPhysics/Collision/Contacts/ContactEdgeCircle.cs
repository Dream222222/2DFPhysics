using FixedPointy;
using TF.Colliders;
using UnityEngine;

namespace TF.Core
{
    //https://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm
    public class ContactEdgeCircle : ContactCallback
    {
        public static readonly ContactEdgeCircle instance = new ContactEdgeCircle();

        public void HandleCollision(Manifold m, TFRigidbody a, TFRigidbody b)
        {
            TFEdgeCollider A = (TFEdgeCollider)a.coll;
            TFCircleCollider B = (TFCircleCollider)b.coll;

            // No line segments, return out.
            if(A.vertices.Count < 2)
            {
                return;
            }

            // Transform circle center to Edge model space
            FixVec2 circleCenter = A.u.Transposed() * (b.Position - a.Position);

            // Iterate through all the line segments to find contact point.
            for (int i = 0; i < A.vertices.Count-1; i++)
            {
                FixVec2 rayDir = (A.vertices[i+1] - A.vertices[i]);
                FixVec2 centerRay = (A.vertices[i]-circleCenter);
                Fix k = rayDir.Dot(rayDir);
                Fix l = 2*centerRay.Dot(rayDir);
                Fix n = centerRay.Dot(centerRay) - (B.radius*B.radius);

                Fix discriminant = l * l - 4 * k * n;
                
                // No intersection.
                if(discriminant <= Fix.zero)
                {
                    continue;
                }

                discriminant = FixMath.Sqrt(discriminant);

                Fix t1 = (-l - discriminant) / (2 * k);
                Fix t2 = (-l + discriminant) / (2 * k);

                Fix s = FixVec2.Dot(A.normals[i], circleCenter - A.vertices[i]);

                if (t1 >= Fix.zero && t1 <= Fix.one)
                {
                    //t1 is the intersection, and it's closer than t2.
                    m.contactCount = 1;
                    m.contacts[0] = (A.u * A.vertices[i] + a.Position) + (t1 * rayDir);
                    m.normal = A.normals[i];
                    m.penetration = B.radius - s;
                    return;
                }

                if(t2 >= Fix.zero && t2 <= Fix.one)
                {
                    // t1 didn't insection, so we either started inside the circle
                    // or completely past it.
                    m.contactCount = 1;
                    m.contacts[0] = (A.u * A.vertices[i] + a.Position) + (t2 * rayDir);
                    m.normal = A.normals[i];
                    m.penetration = B.radius - s;
                    return;
                }
            }
        }
    }
}
