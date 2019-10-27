using FixedPointy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TF.Core
{
    [CreateAssetMenu(fileName = "TFSettings", menuName = "TF/Settings", order = 1)]
    public class TFSettings : ScriptableObject
    {
        public bool AutoSimulation = true;
        public Fix deltaTime = (Fix)(1.0f / 60.0f);
        public FixVec2 gravity;
        public Fix minimumFrictionImpulse;
        public Fix biasRelative = (Fix)(0.95f);
        public Fix biasAbsolute = (Fix)(0.01f);
        public int solveCollisionIterations = 1;
        public TFPhysicsMaterial defaultMaterial;

        [Header("Dynamic Tree")]
        public Fix aabbFattening = 1;
        public Fix aabbMultiplier = 1;
    }
}