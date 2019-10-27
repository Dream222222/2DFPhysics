using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TF.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TF.Colliders
{
    public class TFEdgeCollider : TFCollider
    {
        public static readonly int MAX_POLY_VERTEX_COUNT = 64;

        public List<FixVec2> vertices = new List<FixVec2>();
        public List<FixVec2> normals = new List<FixVec2>();

        protected override void Awake()
        {
            base.Awake();
            CalculateNormals();
        }

        // Used when we only change position.
        public override void MoveAABB(FixVec2 posDiff)
        {
            boundingBox.min.x += posDiff.X;
            boundingBox.min.y += posDiff.Y;
            boundingBox.max.x += posDiff.X;
            boundingBox.max.y += posDiff.Y;
        }

        // Used when we rotate or vertices are added/removed.
        public override void RecalcAABB(FixVec2 pos)
        {
            boundingBox.min.x = pos.X;
            boundingBox.max.x = pos.X;
            boundingBox.min.y = pos.Y;
            boundingBox.max.y = pos.Y;
            for (int i = 0; i < vertices.Count; i++)
            {
                FixVec2 v = u.Transposed() * vertices[i];
                if (v.X + pos.x < boundingBox.min.X)
                {
                    boundingBox.min.x = v.X + pos.x;
                }
                if (v.Y + pos.y < boundingBox.min.Y)
                {
                    boundingBox.min.y = v.Y + pos.y;
                }
                if (v.X + pos.x > boundingBox.max.X)
                {
                    boundingBox.max.x = v.X + pos.x;
                }
                if (v.Y + pos.y > boundingBox.max.Y)
                {
                    boundingBox.max.y = v.Y + pos.y;
                }
            }
        }

        public override TFColliderType GetCType()
        {
            return TFColliderType.Edge;
        }

        public void SetVertices(List<FixVec2> verts)
        {
            // Find the right most point on the hull
            int rightMost = 0;
            Fix highestXCoord = verts[0].X;
            for (int i = 1; i < verts.Count; ++i)
            {
                Fix x = verts[i].X;

                if (x > highestXCoord)
                {
                    highestXCoord = x;
                    rightMost = i;
                }
                // If matching x then take farthest negative y
                else if (x == highestXCoord)
                {
                    if (verts[i].Y < verts[rightMost].Y)
                    {
                        rightMost = i;
                    }
                }
            }

            int[] hull = new int[MAX_POLY_VERTEX_COUNT];
            int outCount = 0;
            int indexHull = rightMost;

            for (; ; )
            {
                hull[outCount] = indexHull;

                // Search for next index that wraps around the hull
                // by computing cross products to find the most counter-clockwise
                // vertex in the set, given the previos hull index
                int nextHullIndex = 0;
                for (int i = 1; i < verts.Count; ++i)
                {
                    // Skip if same coordinate as we need three unique
                    // points in the set to perform a cross product
                    if (nextHullIndex == indexHull)
                    {
                        nextHullIndex = i;
                        continue;
                    }


                    // Cross every set of three unique vertices
                    // Record each counter clockwise third vertex and add
                    // to the output hull
                    // See : http://www.oocities.org/pcgpe/math2d.html
                    FixVec2 e1 = verts[nextHullIndex] - verts[hull[outCount]];
                    FixVec2 e2 = verts[i] - verts[hull[outCount]];
                    Fix c = FixVec2.Cross(e1, e2);
                    if (c < Fix.Zero)
                    {
                        nextHullIndex = i;
                    }

                    // Cross product is zero then e vectors are on same line
                    // therefore want to record vertex farthest along that line
                    if (c == Fix.Zero && e2.GetMagnitudeSquared() > e1.GetMagnitudeSquared())
                    {
                        nextHullIndex = i;
                    }
                }

                ++outCount;
                indexHull = nextHullIndex;

                // Conclude algorithm upon wrap-around
                if (nextHullIndex == rightMost)
                {
                    break;
                }
            }


            // Copy vertices into shape's vertices
            for (int i = 0; i < vertices.Count; ++i)
            {
                vertices[i] = verts[hull[i]];
            }
            CalculateNormals();
        }

        public void CalculateNormals()
        {
            normals = new List<FixVec2>();
            // Compute face normals
            for (int i = 0; i < vertices.Count-1; i++)
            {
                normals.Add(new FixVec2());
                FixVec2 n = vertices[i + 1] - vertices[i];
                normals[i] = new FixVec2(-n.Y, n.X).Normalized();
            }
        }

        public FixVec2 getSupport(FixVec2 dir)
        {
            Fix bestProjection = -Fix.MaxValue;
            FixVec2 bestVertex = new FixVec2(0, 0);


            for (int i = 0; i < vertices.Count; ++i)
            {
                FixVec2 v = vertices[i];
                Fix projection = FixVec2.Dot(v, dir);

                if (projection > bestProjection)
                {
                    bestVertex = v;
                    bestProjection = projection;
                }
            }

            return bestVertex;
        }


#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            FixVec2 pos = (FixVec2)tdTransform.Position;
            // Draw a yellow sphere at the transform's position
            UnityEditor.Handles.color = Color.yellow;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Handles.DrawLine((Vector3)((pos + (tdTransform.rotation * vertices[i]))),
                    (Vector3)((pos + (tdTransform.rotation * vertices[i + 1]))));
            }

            //Draw bounding box.
            UnityEditor.Handles.color = Color.white;
            Handles.DrawLine((Vector3)(boundingBox.min), (Vector3)(boundingBox.max));
        }
#endif
    }
}