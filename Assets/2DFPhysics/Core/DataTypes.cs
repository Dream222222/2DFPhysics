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
        public Fix rotation; // In degrees
        public FixVec2 velocity;
        public Fix angularVelocity;
        public FixVec2 force;
        public Fix torque;
    }

    [System.Serializable]
    public enum InterpolationType {
        None = 0,
        Interpolate = 1,
        Extrapolate = 2
    }
}