using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;

namespace TDFP.Core
{
    [System.Serializable]
    public struct FPRInfo
    {
        public MassData massData;
        public FixVec2 position;
        public Fix rotation;
        public FixVec2 velocity;
        public Fix angularVelocity;
        public FixVec2 force;
        public Fix torque;
        public Fix staticFriction;
        public Fix dynamicFriction;
    }

    [System.Serializable]
    public struct MassData
    {
        public Fix mass;
        public Fix inv_mass;

        // For rotations (not covered in this article)
        public Fix inertia;
        public Fix inverse_inertia;
    };
}