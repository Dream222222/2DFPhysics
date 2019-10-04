using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TDFP.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDFP.Colliders
{
    public class TFPPolygonCollider : TFPCollider
    {
        public static readonly int MAX_POLY_VERTEX_COUNT = 64;

        public int vertexCount;
        public List<FixVec2> vertices = new List<FixVec2>();
        public List<FixVec2> normals = new List<FixVec2>();

        protected override void Awake()
        {
            base.Awake();
            CalculateNormals();
        }

        public override void UpdateAABB(FixVec2 posDiff)
        {
            boundingBox.min._x += posDiff.X;
            boundingBox.min._y += posDiff.Y;
            boundingBox.max._x += posDiff.X;
            boundingBox.max._y += posDiff.Y;
        }

        public override void RecalcAABB(FixVec2 pos)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].X < boundingBox.min.X)
                {
                    boundingBox.min._x = vertices[i].X;
                }
                if (vertices[i].Y < boundingBox.min.Y)
                {
                    boundingBox.min._y = vertices[i].Y;
                }
                if (vertices[i].X > boundingBox.max.X)
                {
                    boundingBox.max._x = vertices[i].X;
                }
                if (vertices[i].Y > boundingBox.max.Y)
                {
                    boundingBox.max._y = vertices[i].Y;
                }
            }
        }

        public override TFPColliderType GetCType()
        {
            return TFPColliderType.Polygon;
        }

        public void set(List<FixVec2> verts)
        {
            // Find the right most point on the hull
            int rightMost = 0;
            Fix highestXCoord = verts[0].X;
            for(int i = 1; i < verts.Count; ++i)
            {
                Fix x = verts[i].X;

                if(x > highestXCoord)
                {
                    highestXCoord = x;
                    rightMost = i;
                }
                // If matching x then take farthest negative y
                else if(x == highestXCoord)
                {
                    if(verts[i].Y < verts[rightMost].Y)
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
                    vertexCount = outCount;
                    break;
                }
            }


            // Copy vertices into shape's vertices
            for (int i = 0; i < vertexCount; ++i)
            {
                vertices[i] = verts[hull[i]];
            }

            CalculateNormals();
        }

        public void CalculateNormals()
        {
            // Compute face normals
            for (int i = 0; i < vertexCount; ++i)
            {
                FixVec2 face = vertices[(i + 1) % vertexCount] - vertices[i];

                // Calculate normal with 2D cross product between vector and scalar
                normals[i] = new FixVec2(face.Y, -face.X);
                normals[i].Normalize();
            }
        }

        public FixVec2 getSupport(FixVec2 dir)
        {
            Fix bestProjection = -Fix.MaxValue;
            FixVec2 bestVertex = new FixVec2(0, 0);

            for (int i = 0; i < vertexCount; ++i)
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
            // Draw a yellow sphere at the transform's position
            UnityEditor.Handles.color = Color.yellow;
            for(int i = 0; i < vertices.Count-1; i++)
            {
                Handles.DrawLine((Vector3)(tdTransform.Position+vertices[i]), (Vector3)(tdTransform.Position+vertices[i+1]));
            }
        }
#endif
    }
}
