using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;
using Unity.Collections;

namespace TDFP.Core
{
    [System.Serializable]
    public struct FPRInfo
    {
        public FixVec2 position;
        public Fix rotation; // In radians
        public FixVec2 velocity;
        public Fix angularVelocity;
        public FixVec2 force;
        public Fix torque;
    }
}