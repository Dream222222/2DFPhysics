using System;
using FixedPointy;
using TDFP.Colliders;

namespace TDFP.Core
{
    public class CollisionPolygonPolygon : CollisionCallback
    {
        public static readonly CollisionPolygonPolygon instance = new CollisionPolygonPolygon();

        public void HandleCollision(Manifold m, FPRigidbody a, FPRigidbody b)
        {
            TFPPolygonCollider A = (TFPPolygonCollider)a.coll;
            TFPPolygonCollider B = (TFPPolygonCollider)b.coll;

            m.contactCount = 0;

            // Check for a separating axis with A's face planes
            int[] faceA = { 0 };
            Fix penetrationA = FindAxisLeastPenetration(faceA, A, B);
            if (penetrationA >= Fix.Zero)
            {
                return;
            }

            int[] faceB = { 0 };
            Fix penetrationB = FindAxisLeastPenetration(faceB, B, A);
            if (penetrationB >= Fix.Zero)
            {
                return;
            }

            int referenceIndex;
            bool flip; //Always point from a to b


            TFPPolygonCollider refPoly; //Reference
            TFPPolygonCollider incPoly; //Incident

            //Determine which shape contains reference face
            if (TDFPhysics.instance.BiasGreaterThan(penetrationA, penetrationB)) {
                refPoly = A;
                incPoly = B;
                referenceIndex = faceA[0];
                flip = false;
            } else
            {
                refPoly = B;
                incPoly = A;
                referenceIndex = faceB[0];
                flip = true;
            }

            // World space incident face
            FixVec2[] incidentFace = new FixVec2[2];
            FindIncidentFace(incidentFace, refPoly, incPoly, referenceIndex);

            // Setup reference face certices
            FixVec2 v1 = refPoly.vertices[referenceIndex];
            referenceIndex = referenceIndex + 1 == refPoly.vertexCount ? 0 : referenceIndex + 1;
            FixVec2 v2 = refPoly.vertices[referenceIndex];

            // Transform vertices to world space
            v1 = refPoly.u * v1 + refPoly.body.info.position;
            v2 = refPoly.u * v2 + refPoly.body.info.position;

            //Calculate reference face side normal in world space
            FixVec2 sidePlaneNormal = v2 - v1;
            sidePlaneNormal = sidePlaneNormal.Normalized();

            // Orthogonalize
            FixVec2 refFaceNormal = new FixVec2(sidePlaneNormal.Y, -sidePlaneNormal.X);

            // ax + by = c
            // c is distance from origin
            Fix refC = FixVec2.Dot(refFaceNormal, v1);
            Fix negSide = -FixVec2.Dot(sidePlaneNormal, v1);
            Fix posSide = FixVec2.Dot(sidePlaneNormal, v2);

            // Clip incident face to reference face side planes
            if (Clip(-sidePlaneNormal, negSide, incidentFace) < 2)
            {
                return; // Due to floating point error, possible to not have required points
            }

            if (Clip(sidePlaneNormal, posSide, incidentFace) < 2)
            {
                return;
            }

            // Flip
            m.normal = flip ? -refFaceNormal : refFaceNormal;

            // Keep points behind reference face
            int cp = 0; // clipped points behind reference face
            Fix separation = FixVec2.Dot(refFaceNormal, incidentFace[0]) - refC;
            if (separation <= Fix.Zero)
            {
                m.contacts[cp] = incidentFace[0];
                m.penetration = -separation;
                ++cp;
            }
            else
            {
                m.penetration = 0;
            }

            separation = FixVec2.Dot(refFaceNormal, incidentFace[1]) - refC;
            if (separation <= Fix.Zero)
            {
                m.contacts[cp] = incidentFace[1];
                m.penetration += -separation;
                ++cp;

                // Average penetration
                m.penetration /= cp;
            }

            m.contactCount = cp;
        }

        private int Clip(FixVec2 n, Fix c, FixVec2[] face)
        {
            int sp = 0;
            FixVec2[] o = { face[0], face[1] };

            // Retrieve distances from each endpoint to the line
            Fix d1 = FixVec2.Dot(n, face[0]) - c;
            Fix d2 = FixVec2.Dot(n, face[1]) - c;

            // If negative (behind plane) clip
            if (d1 <= Fix.Zero) o[sp++] = face[0];
            if (d2 <= Fix.Zero) o[sp++] = face[1];

            // If the points are on different sides of the plane
            if (d1 * d2 < Fix.Zero) // less than to ignore -0.0f
            {
                // Push interesection point
                Fix alpha = d1 / (d1 - d2);
                o[sp] = face[0] + alpha* (face[1] - face[0]);
                ++sp;
            }

            // Assign our new converted values
            face[0] = o[0];
            face[1] = o[1];

            return sp;
        }

        private void FindIncidentFace(FixVec2[] v, TFPPolygonCollider refPoly, TFPPolygonCollider incPoly, int referenceIndex)
        {
            FixVec2 referenceNormal = refPoly.normals[referenceIndex];

            // Calculate normal in incident's frame of reference
            referenceNormal = refPoly.u * referenceNormal; // To world space
            referenceNormal = incPoly.u.Transposed() * referenceNormal; // To incident's model space

            // Find most anti-normal face on incident polygon
            int incidentFace = 0;
            Fix minDot = Fix.MaxValue;
            for (int i = 0; i < incPoly.vertexCount; ++i)
            {
                Fix dot = FixVec2.Dot(referenceNormal, incPoly.normals[i]);
                if (dot < minDot)
                {
                    minDot = dot;
                    incidentFace = i;
                }
            }

            // Assign face vertices for incidentFace
            v[0] = incPoly.u * incPoly.vertices[incidentFace] + incPoly.body.info.position;
            incidentFace = incidentFace + 1 >= (int)incPoly.vertexCount ? 0 : incidentFace + 1;
            v[1] = incPoly.u * incPoly.vertices[incidentFace] + incPoly.body.info.position;
        }

        public Fix FindAxisLeastPenetration(int[] faceIndex, TFPPolygonCollider A, TFPPolygonCollider B)
        {
            Fix bestDistance = -Fix.MaxValue;
            int bestIndex = 0;

            for (int i = 0; i < A.vertexCount; ++i)
            {
                // Retrieve a face normal from A
                FixVec2 nw = A.u * A.normals[i];

                // Transform face normal into B's model space
                Mat22 buT = B.u.Transposed();
                FixVec2 n = buT * nw;

                // Retrieve support point from B along -n
                // Vec2 s = B->GetSupport( -n );
                FixVec2 s = B.getSupport(-n);

                // Retrieve vertex on face from A, transform into
                FixVec2 v = A.vertices[i];
                v = A.u * v + A.body.Position;
                v -= B.body.info.position;
                v = buT * v;

                // Compute penetration distance (in B's model space)
                Fix d = FixVec2.Dot(n, s - v);

                // Store greatest distance
                if (d > bestDistance)
                {
                    bestDistance = d;
                    bestIndex = i;
                }
            }

            faceIndex[0] = bestIndex;
            return bestDistance;
        }
    }
}