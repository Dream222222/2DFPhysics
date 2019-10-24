using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

/// <summary>
/// Based primarily on box2d's implementation.
/// </summary>
namespace TDFP.Core
{
    public class DynamicTree
    {
        public static int nullNode = -1;

        public List<DTNode> nodes;
        public int nodeCount; // The number of nodes in the tree.
        public int nodeCapacity; // The number of nodes (including free nodes).
        public int rootIndex; // The index of the root node.
        public int freeList; // The index of the root of free nodes.

        public DynamicTree()
        {
            rootIndex = nullNode;
            freeList = nullNode;
            
            nodeCapacity = 16;
            nodeCount = 0;
            nodes = new List<DTNode>(nodeCapacity);

            // Build a linked list for the free list.
            for (var i = 0; i < nodeCapacity; ++i)
            {
                nodes[i].nextNodeIndex = i + 1;
                nodes[i].height = -1;
            }

            nodes[nodeCapacity - 1].nextNodeIndex = nullNode;
            nodes[nodeCapacity - 1].height = -1;
            freeList = 0;
        }

        /// <summary>
        /// Creates a proxy in the tree as a leaf node.
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="bodyIndex"></param>
        /// <returns></returns>
        public int CreateProxy(AABB aabb, int bodyIndex)
        {
            int proxyId = AllocateNode();

            // Fatten the aabb.
            FixVec2 r = new FixVec2(TDFPhysics.instance.settings.aabbFattening, TDFPhysics.instance.settings.aabbFattening);
            nodes[proxyId].aabb.min = aabb.min - r;
            nodes[proxyId].aabb.max = aabb.max + r;
            nodes[proxyId].bodyIndex = bodyIndex;
            nodes[proxyId].height = 0;

            InsertLeaf(proxyId);

            return proxyId;
        }

        public void DestroyProxy(int proxyId)
        {
            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        /// <summary>
        /// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted.
        /// </summary>
        /// <param name="proxyId">The ID of the proxy.</param>
        /// <param name="aabb">The AABB of the proxy.</param>
        /// <param name="displacement">How much the proxy was displaced.</param>
        /// <returns>True if the proxy was reinserted. </returns>
        public bool MoveProxy(int proxyId, AABB aabb, FixVec2 displacement)
        {
            // If the node is still inside the bounds, don't bother moving it.
            if (nodes[proxyId].aabb.Contains(aabb))
            {
                return false;
            }

            RemoveLeaf(proxyId);


            // Extend AABB.
            var b = aabb;
            FixVec2 r = new FixVec2(TDFPhysics.instance.settings.aabbFattening, TDFPhysics.instance.settings.aabbFattening);
            b.min = b.min - r;
            b.max = b.max + r;

            // Predict AABB displacement.
            FixVec2 d = TDFPhysics.instance.settings.aabbMultiplier * displacement;

            if (d.X < Fix.Zero)
            {
                b.min._x += d._x;
            }
            else
            {
                b.max._x += d._x;
            }

            if (d.Y < Fix.Zero)
            {
                b.min._y += d.Y;
            }
            else
            {
                b.max._y += d.Y;
            }

            nodes[proxyId].aabb = b;

            InsertLeaf(proxyId);
            return true;
        }

        public void ShiftOrigin(FixVec2 newOrigin)
        {
            // Build array of leaves. Free the rest.
            for (int i = 0; i < nodeCapacity; ++i)
            {
                nodes[i].aabb.min -= newOrigin;
                nodes[i].aabb.max -= newOrigin;
            }
        }

        /// <summary>
        /// Compute the cost of the tree. This gets the area of nodes, ignoring leaves. 
        /// </summary>
        public Fix ComputeCost()
        {
            Fix cost = 0;
            for (int i = 0; i < nodeCount; i++)
            {
                if (!nodes[i].IsLeaf())
                {
                    cost += AABB.Area(nodes[i].aabb);
                }
            }
            return cost;
        }

        // Insert a leaf into the tree.
        private void InsertLeaf(int leafIndex)
        {
            if(rootIndex == nullNode)
            {
                rootIndex = leafIndex;
                nodes[rootIndex].parentIndex = nullNode;
                return;
            }

            // STEP 1: Find the best sibling for the leaf.
            int bestSibling = 0;
            bestSibling = PickBestSibling(leafIndex);

            // STEP 2: Create a new parent.
            int oldParent = nodes[bestSibling].parentIndex;
            int newParent = AllocateNode();
            nodes[newParent].parentIndex = oldParent;
            nodes[newParent].aabb = AABB.Union(nodes[leafIndex].aabb, nodes[bestSibling].aabb);
            nodes[newParent].height = nodes[bestSibling].height + 1;

            if(oldParent != -1)
            {
                // The sibling was not the root.
                if (nodes[oldParent].leftChildIndex == bestSibling)
                {
                    nodes[oldParent].leftChildIndex = newParent;
                }
                else
                {
                    nodes[oldParent].rightChildIndex = newParent;
                }

                nodes[newParent].leftChildIndex = bestSibling;
                nodes[newParent].rightChildIndex = leafIndex;
                nodes[bestSibling].parentIndex = newParent;
                nodes[leafIndex].parentIndex = newParent;
            }
            else
            {
                // The sibling was the root.
                nodes[newParent].leftChildIndex = bestSibling;
                nodes[newParent].rightChildIndex = leafIndex;
                nodes[bestSibling].parentIndex = newParent;
                nodes[leafIndex].parentIndex = newParent;
                rootIndex = newParent;
            }

            // STEP 3: Walk back up the tree refitting AABBs.
            int walkIndex = nodes[leafIndex].parentIndex;
            while(walkIndex != -1)
            {
                int child1 = nodes[walkIndex].leftChildIndex;
                int child2 = nodes[walkIndex].rightChildIndex;

                nodes[walkIndex].height = 1 + Mathf.Max(nodes[child1].height, nodes[child2].height);
                nodes[walkIndex].aabb = AABB.Union(nodes[child1].aabb, nodes[child2].aabb);

                Rotate(walkIndex);

                walkIndex = nodes[walkIndex].parentIndex;
            }
        }

        /// <summary>
        /// Remove a leaf from the tree.
        /// </summary>
        /// <param name="leafIndex">The index of the leaf to remove.</param>
        private void RemoveLeaf(int leafIndex)
        {
            if(leafIndex == rootIndex)
            {
                rootIndex = -1;
                return;
            }

            int parent = nodes[leafIndex].parentIndex;
            int grandParent = nodes[parent].parentIndex;
            int sibling;
            if (nodes[parent].leftChildIndex == leafIndex)
            {
                sibling = nodes[parent].rightChildIndex;
            }
            else
            {
                sibling = nodes[parent].leftChildIndex;
            }

            if (grandParent != -1)
            {
                // Destroy parent and connect sibling to grandParent.
                if (nodes[grandParent].leftChildIndex == parent)
                {
                    nodes[grandParent].leftChildIndex = sibling;
                }
                else
                {
                    nodes[grandParent].rightChildIndex = sibling;
                }
                nodes[sibling].parentIndex = grandParent;
                FreeNode(parent);

                // Adjust ancestor bounds.
                int index = grandParent;
                while (index != nullNode)
                {
                    index = Rotate(index);

                    int childLeft = nodes[index].leftChildIndex;
                    int childRight = nodes[index].rightChildIndex;

                    nodes[index].aabb = AABB.Union(nodes[childLeft].aabb, nodes[childRight].aabb);
                    nodes[index].height = 1 + Mathf.Max(nodes[childLeft].height, nodes[childRight].height);

                    index = nodes[index].parentIndex;
                }
            }
            else
            {
                rootIndex = sibling;
                nodes[sibling].parentIndex = nullNode;
                FreeNode(parent);
            }
        }

        /// <summary>
        /// Performs a left/right rotation if node A is imbalanced.
        /// </summary>
        /// <param name="indexA">The index of node A.</param>
        /// <returns>The new root index.</returns>
        private int Rotate(int indexA)
        {
            DTNode A = nodes[indexA];
            if (A.IsLeaf() || A.height < 2)
            {
                return indexA;
            }

            int iB = A.leftChildIndex;
            int iC = A.rightChildIndex;

            DTNode B = nodes[iB];
            DTNode C = nodes[iC];

            int balance = C.height - B.height;

            // Rotate C up.
            if(balance > 1)
            {
                int iF = C.leftChildIndex;
                int iG = C.rightChildIndex;
                DTNode F = nodes[iF];
                DTNode G = nodes[iG];

                // Swap A and C
                C.leftChildIndex = indexA;
                C.parentIndex = A.parentIndex;
                A.parentIndex = iC;

                // A's old parent should point to C
                if (C.parentIndex != nullNode)
                {
                    if (nodes[C.parentIndex].leftChildIndex == indexA)
                    {
                        nodes[C.parentIndex].leftChildIndex = iC;
                    }
                    else
                    {
                        nodes[C.parentIndex].rightChildIndex = iC;
                    }
                }
                else
                {
                    rootIndex = iC;
                }


                // Rotate
                if (F.height > G.height)
                {
                    C.rightChildIndex = iF;
                    A.rightChildIndex = iG;
                    G.parentIndex = indexA;
                    A.aabb = AABB.Union(B.aabb, G.aabb);
                    C.aabb = AABB.Union(A.aabb, F.aabb);

                    A.height = 1 + Mathf.Max(B.height, G.height);
                    C.height = 1 + Mathf.Max(A.height, F.height);
                }
                else
                {
                    C.rightChildIndex = iG;
                    A.rightChildIndex = iF;
                    F.parentIndex = indexA;
                    A.aabb = AABB.Union(B.aabb, F.aabb);
                    C.aabb = AABB.Union(A.aabb, G.aabb);

                    A.height = 1 + Mathf.Max(B.height, F.height);
                    C.height = 1 + Mathf.Max(A.height, G.height);
                }

                return iC;
            }

            // Rotate B up
            if (balance < -1)
            {
                int iD = B.leftChildIndex;
                int iE = B.rightChildIndex;
                DTNode D = nodes[iD];
                DTNode E = nodes[iE];

                // Swap A and B
                B.leftChildIndex = indexA;
                B.parentIndex = A.parentIndex;
                A.parentIndex = iB;

                // A's old parent should point to B
                if (B.parentIndex != nullNode)
                {
                    if (nodes[B.parentIndex].leftChildIndex == indexA)
                    {
                        nodes[B.parentIndex].leftChildIndex = iB;
                    }
                    else
                    {
                        nodes[B.parentIndex].rightChildIndex = iB;
                    }
                }
                else
                {
                    rootIndex = iB;
                }

                // Rotate
                if (D.height > E.height)
                {
                    B.rightChildIndex = iD;
                    A.leftChildIndex = iE;
                    E.parentIndex = indexA;
                    A.aabb = AABB.Union(C.aabb, E.aabb);
                    B.aabb = AABB.Union(A.aabb, D.aabb);

                    A.height = 1 + Mathf.Max(C.height, E.height);
                    B.height = 1 + Mathf.Max(A.height, D.height);
                }
                else
                {
                    B.rightChildIndex = iE;
                    A.leftChildIndex = iD;
                    D.parentIndex = indexA;
                    A.aabb = AABB.Union(C.aabb, D.aabb);
                    B.aabb = AABB.Union(A.aabb, E.aabb);

                    A.height = 1 + Mathf.Max(C.height, D.height);
                    B.height = 1 + Mathf.Max(A.height, E.height);
                }

                return iB;
            }

            return indexA;
        }

        /// <summary>
        /// Pick the best sibling for a leaf node using "Branch and Bound" algorithm.
        /// </summary>
        /// <param name="leafIndex">The index of the leaf node we want a sibling for.</param>
        private int PickBestSibling(int leafIndex)
        {
            int index = rootIndex;
            while (!nodes[index].IsLeaf())
            {
                int childLeft = nodes[index].leftChildIndex;
                int childRight = nodes[index].rightChildIndex;

                Fix area = AABB.Area(nodes[index].aabb);

                AABB combinedAABB = AABB.Union(nodes[index].aabb, nodes[leafIndex].aabb);
                Fix combinedArea = AABB.Area(combinedAABB);

                // Cost of creating a new parent for this node and the new leaf
                Fix cost = 2 * combinedArea;

                // Minimum cost of pushing the leaf further down the tree
                Fix inheritanceCost = 2 * (combinedArea - area);

                // Cost of descending into left child
                Fix costLeft;
                if (nodes[childLeft].IsLeaf())
                {
                    AABB aabb = AABB.Union(nodes[leafIndex].aabb, nodes[childLeft].aabb);
                    costLeft = AABB.Area(aabb) + inheritanceCost;
                }
                else
                {
                    AABB aabb = AABB.Union(nodes[leafIndex].aabb, nodes[childLeft].aabb);
                    Fix oldArea = AABB.Area(nodes[childLeft].aabb);
                    Fix newArea = AABB.Area(aabb);
                    costLeft = (newArea - oldArea) + inheritanceCost;
                }

                // Cost of descending into right child
                Fix costRight;
                if (nodes[childRight].IsLeaf())
                {
                    AABB aabb = AABB.Union(nodes[leafIndex].aabb, nodes[childRight].aabb);
                    costRight = AABB.Area(aabb) + inheritanceCost;
                }
                else
                {
                    AABB aabb = AABB.Union(nodes[leafIndex].aabb, nodes[childRight].aabb);
                    Fix oldArea = AABB.Area(nodes[childRight].aabb);
                    Fix newArea = AABB.Area(aabb);
                    costRight = (newArea - oldArea) + inheritanceCost;
                }

                // Descend according to the minimum cost.
                if (cost < costLeft && cost < costRight)
                {
                    break;
                }

                // Descend
                if (costLeft < costRight)
                {
                    index = childLeft;
                }
                else
                {
                    index = childRight;
                }
            }
            return index;
        }

        /// <summary>
        /// Gets a free node from the pool.
        /// </summary>
        /// <returns></returns>
        private int AllocateNode()
        {
            // Expand the node pool as needed.
            if (freeList == nullNode)
            {
                // Free list is empty. Rebuild a bigger pool.
                nodeCapacity *= 2;
                while (nodes.Count < nodeCapacity)
                {
                    nodes.Add(new DTNode());
                }

                // Build a linked list for the free list.
                for (int i = nodeCount; i < nodeCapacity; ++i)
                {
                    nodes[i].nextNodeIndex = i + 1;
                    nodes[i].height = -1;
                }

                nodes[nodeCapacity - 1].nextNodeIndex = nullNode;
                nodes[nodeCapacity - 1].height = -1;
                freeList = nodeCount;
            }

            // Peel a node off the free list.
            int nodeId = freeList;
            freeList = nodes[nodeId].nextNodeIndex;
            nodes[nodeId].parentIndex = nullNode;
            nodes[nodeId].leftChildIndex = nullNode;
            nodes[nodeId].rightChildIndex = nullNode;
            nodes[nodeId].height = 0;
            nodes[nodeId].bodyIndex = -1;
            ++nodeCount;
            return nodeId;
        }

        /// <summary>
        /// Returns a node to the pool.
        /// </summary>
        /// <param name="nodeIndex"></param>
        private void FreeNode(int nodeIndex)
        {
            nodes[nodeIndex].nextNodeIndex = freeList;
            nodes[nodeIndex].height = -1;
            freeList = nodeIndex;
            --nodeCount;
        }
    }
}