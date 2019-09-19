using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;

namespace TDFP.Colliders
{
    public class TDFPBoxCollider : TDFPCollider
    {
        public FixVec2 size;
        public FixConst edgeRadius;
    }
}
