using UnityEngine;
using FixedPointy;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

/// <summary>
/// Based primarily on box2d's implementation.
/// </summary>
namespace TF.Core
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
            
            nodeCapacity = 16;
            nodeCount = 0;
            nodes = new List<DTNode>(nodeCapacity);

            // Build a linked list for the free list.
            for (var i = 0; i < nodeCapacity; ++i)
            {
                nodes.Add(new DTNode());
                nodes[i].nextNodeIndex = i + 1;
                nodes[i].height = -1;
            }

            nodes[nodeCapacity - 1].nextNodeIndex = nullNode;
            nodes[nodeCapacity - 1].height = -1;
            freeList = 0;
        }

        /// <summary>
        /// Creates a proxy in the tree as a leaf node.
        /// The AABB is fattened before being inserted.
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="bodyIndex"></param>
        /// <returns>The proxy ID.</returns>
        public int CreateProxy(AABB aabb, int bodyIndex, Fix aabbFattening)
        {
            int proxyId = AllocateNode();

            // Fatten the aabb.
            FixVec2 r = new FixVec2(aabbFattening, aabbFattening);
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

        public AABB GetFatAABB(int getPairsProxyID)
        {
            return nodes[getPairsProxyID].aabb;
        }

        /// <summary>
        /// Move a proxy with a swepted AABB. 
        /// If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted.
        /// </summary>
        /// <param name="proxyId">The ID of the proxy.</param>
        /// <param name="aabb">The AABB of the proxy.</param>
        /// <param name="displacement">How much the proxy was displaced.</param>
        /// <returns>True if the proxy was reinserted.</returns>
        public bool MoveProxy(int proxyId, AABB aabb, FixVec2 displacement)
        {
            // The proxy is still within it's fattened AABB, don't move it.
            if (nodes[proxyId].aabb.Contains(aabb))
            {
                return false;
            }

            RemoveLeaf(proxyId);


            // Extend AABB.
            AABB b = aabb;
            FixVec2 r = new FixVec2(TFPhysics.instance.settings.aabbFattening, TFPhysics.instance.settings.aabbFattening);
            b.min = b.min - r;
            b.max = b.max + r;

            // Predict AABB displacement.
            FixVec2 d = TFPhysics.instance.settings.aabbMultiplier * displacement;

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

        /// <summary>
        /// Query an AABB for overlapping proxies.
        /// </summary>
        /// <param name="aabb"></param>
        public void Query(ITreeQueryCallback callback, AABB aabb)
        {
            Stack<int> stack = new Stack<int>();
            stack.Push(rootIndex);

            while (stack.Count > 0)
            {
                int nodeId = stack.Pop();
                if (nodeId == nullNode)
                {
                    continue;
                }

                DTNode node = nodes[nodeId];
                if (node.aabb.Overlaps(aabb))
                {
                    if (node.IsLeaf())
                    {
                        bool proceed = callback.QueryCallback(nodeId);
                        if (proceed == false)
                        {
                            return;
                        }
                    }
                    else
                    {
                        stack.Push(node.leftChildIndex);
                        stack.Push(node.rightChildIndex);
                    }
                }
            }
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

            AABB leafAABB = nodes[leafIndex].aabb;
            // STEP 1: Find the best sibling for the leaf.
            int bestSibling = rootIndex;
            bestSibling = PickBestSibling(leafAABB);

            // STEP 2: Create a new parent.
            int oldParent = nodes[bestSibling].parentIndex;
            int newParent = AllocateNode();
            nodes[newParent].parentIndex = oldParent;
            nodes[newParent].bodyIndex = nullNode;
            nodes[newParent].aabb = AABB.Union(leafAABB, nodes[bestSibling].aabb);
            nodes[newParent].height = nodes[bestSibling].height + 1;

            if(oldParent != nullNode)
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
            while(walkIndex != nullNode)
            {
                walkIndex = Rotate(walkIndex);

                int child1 = nodes[walkIndex].leftChildIndex;
                int child2 = nodes[walkIndex].rightChildIndex;

                nodes[walkIndex].height = 1 + Mathf.Max(nodes[child1].height, nodes[child2].height);
                nodes[walkIndex].aabb = AABB.Union(nodes[child1].aabb, nodes[child2].aabb);

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

                    nodes[index].height = 1 + Mathf.Max(nodes[childLeft].height, nodes[childRight].height);
                    nodes[index].aabb = AABB.Union(nodes[childLeft].aabb, nodes[childRight].aabb);

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
            DTNode node = nodes[indexA];
            if (node.IsLeaf() || node.height < 2)
            {
                return indexA;
            }

            int indexLeft = node.leftChildIndex;
            int indexRight = node.rightChildIndex;

            DTNode leftChild = nodes[indexLeft];
            DTNode rightChild = nodes[indexRight];

            int balance = rightChild.height - leftChild.height;

            // Rotate right branch up.
            if(balance > 1)
            {
                int iF = rightChild.leftChildIndex;
                int iG = rightChild.rightChildIndex;
                DTNode F = nodes[iF];
                DTNode G = nodes[iG];

                // Swap node and it's right child.
                rightChild.leftChildIndex = indexA;
                rightChild.parentIndex = node.parentIndex;
                node.parentIndex = indexRight;

                // The node's old parent should point to its right child.
                if (rightChild.parentIndex != nullNode)
                {
                    if (nodes[rightChild.parentIndex].leftChildIndex == indexA)
                    {
                        nodes[rightChild.parentIndex].leftChildIndex = indexRight;
                    }
                    else
                    {
                        nodes[rightChild.parentIndex].rightChildIndex = indexRight;
                    }
                }
                else
                {
                    rootIndex = indexRight;
                }


                // Rotate
                if (F.height > G.height)
                {
                    rightChild.rightChildIndex = iF;
                    node.rightChildIndex = iG;
                    G.parentIndex = indexA;
                    node.aabb = AABB.Union(leftChild.aabb, G.aabb);
                    rightChild.aabb = AABB.Union(node.aabb, F.aabb);

                    node.height = 1 + Mathf.Max(leftChild.height, G.height);
                    rightChild.height = 1 + Mathf.Max(node.height, F.height);
                }
                else
                {
                    rightChild.rightChildIndex = iG;
                    node.rightChildIndex = iF;
                    F.parentIndex = indexA;
                    node.aabb = AABB.Union(leftChild.aabb, F.aabb);
                    rightChild.aabb = AABB.Union(node.aabb, G.aabb);

                    node.height = 1 + Mathf.Max(leftChild.height, F.height);
                    rightChild.height = 1 + Mathf.Max(node.height, G.height);
                }

                return indexRight;
            }

            // Rotate left branch up
            if (balance < -1)
            {
                int iD = leftChild.leftChildIndex;
                int iE = leftChild.rightChildIndex;
                DTNode D = nodes[iD];
                DTNode E = nodes[iE];

                // Swap node and its left child.
                leftChild.leftChildIndex = indexA;
                leftChild.parentIndex = node.parentIndex;
                node.parentIndex = indexLeft;

                // A's old parent should point to B
                if (leftChild.parentIndex != nullNode)
                {
                    if (nodes[leftChild.parentIndex].leftChildIndex == indexA)
                    {
                        nodes[leftChild.parentIndex].leftChildIndex = indexLeft;
                    }
                    else
                    {
                        nodes[leftChild.parentIndex].rightChildIndex = indexLeft;
                    }
                }
                else
                {
                    rootIndex = indexLeft;
                }

                // Rotate
                if (D.height > E.height)
                {
                    leftChild.rightChildIndex = iD;
                    node.leftChildIndex = iE;
                    E.parentIndex = indexA;
                    node.aabb = AABB.Union(rightChild.aabb, E.aabb);
                    leftChild.aabb = AABB.Union(node.aabb, D.aabb);

                    node.height = 1 + Mathf.Max(rightChild.height, E.height);
                    leftChild.height = 1 + Mathf.Max(node.height, D.height);
                }
                else
                {
                    leftChild.rightChildIndex = iE;
                    node.leftChildIndex = iD;
                    D.parentIndex = indexA;
                    node.aabb = AABB.Union(rightChild.aabb, D.aabb);
                    leftChild.aabb = AABB.Union(node.aabb, E.aabb);

                    node.height = 1 + Mathf.Max(rightChild.height, D.height);
                    leftChild.height = 1 + Mathf.Max(node.height, E.height);
                }

                return indexLeft;
            }

            return indexA;
        }

        /// <summary>
        /// Pick the best sibling for a leaf node using "Branch and Bound" algorithm.
        /// </summary>
        /// <param name="leafIndex">The index of the leaf node we want a sibling for.</param>
        private int PickBestSibling(AABB leafAABB)
        {
            int index = rootIndex;
            while (!nodes[index].IsLeaf())
            {
                int childLeft = nodes[index].leftChildIndex;
                int childRight = nodes[index].rightChildIndex;

                Fix area = nodes[index].aabb.Area();

                AABB combinedAABB = AABB.Union(nodes[index].aabb, leafAABB);
                Fix combinedArea = combinedAABB.Area();

                // Cost of creating a new parent for this node and the new leaf
                Fix cost = 2 * combinedArea;

                // Minimum cost of pushing the leaf further down the tree
                Fix inheritanceCost = 2 * (combinedArea - area);

                // Cost of descending into left child
                Fix costLeft;
                if (nodes[childLeft].IsLeaf())
                {
                    AABB aabb = AABB.Union(leafAABB, nodes[childLeft].aabb);
                    costLeft = aabb.Area() + inheritanceCost;
                }
                else
                {
                    AABB aabb = AABB.Union(leafAABB, nodes[childLeft].aabb);
                    Fix oldArea = nodes[childLeft].aabb.Area();
                    Fix newArea = aabb.Area();
                    costLeft = (newArea - oldArea) + inheritanceCost;
                }

                // Cost of descending into right child
                Fix costRight;
                if (nodes[childRight].IsLeaf())
                {
                    AABB aabb = AABB.Union(leafAABB, nodes[childRight].aabb);
                    costRight = AABB.Area(aabb) + inheritanceCost;
                }
                else
                {
                    AABB aabb = AABB.Union(leafAABB, nodes[childRight].aabb);
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
                for (int i = nodeCount; i < nodeCapacity-1; ++i)
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