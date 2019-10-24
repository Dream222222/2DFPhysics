using FixedPointy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDFP.Core
{
    [CreateAssetMenu(fileName = "TDFPSettings", menuName = "TDFP/Settings", order = 1)]
    public class TDFPSettings : ScriptableObject
    {
        public bool AutoSimulation = true;
        public Fix deltaTime = (Fix)(1.0f / 60.0f);
        public FixVec2 gravity;
        public Fix minimumFrictionImpulse;
        public Fix biasRelative = (Fix)(0.95f);
        public Fix biasAbsolute = (Fix)(0.01f);
        public int solveCollisionIterations = 1;
        public FPhysicsMaterial defaultMaterial;
        [Header("Spatial Grid")]
        public FixVec2 gridMinPosition;
        public FixVec2 gridMaxPosition;
        public FixVec2 gridCellSize;
        [Header("Dynamic Tree")]
        public Fix aabbFattening = 1;
        public Fix aabbMultiplier = 1;
    }
}