using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using TDFP.Core;

namespace TDFP.Colliders
{
    public class TDFPCollider : MonoBehaviour
    {
        public AABB bounds;
        public bool isTrigger;
        public FixVec2 offset;
    }
}