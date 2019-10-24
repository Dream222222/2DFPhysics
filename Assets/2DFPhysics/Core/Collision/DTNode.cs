using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDFP.Core
{
    public class DTNode
    {
        public AABB aabb;
        public int bodyIndex = -1;
        public int parentIndex = -1;
        public int nextNodeIndex = -1;

        public int leftChildIndex = -1;
        public int rightChildIndex = -1;

        public int height = -1; // Height of the node. 0 for a leaf, -1 for a free node.

        public bool IsLeaf()
        {
            return leftChildIndex == -1;
        }
    }
}
