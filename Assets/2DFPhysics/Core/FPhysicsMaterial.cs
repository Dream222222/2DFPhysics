using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;

namespace TDFP.Core
{
    [CreateAssetMenu(fileName = "TDFPhysicsMaterial", menuName = "TDFP/PhysicsMaterial", order = 1)]
    public class FPhysicsMaterial : ScriptableObject
    {
        public FixConst bounciness;
        public FixConst friction;
    }
}