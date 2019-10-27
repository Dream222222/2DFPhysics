using UnityEngine;
using FixedPointy;

namespace TF.Core
{
    [CreateAssetMenu(fileName = "TDFPhysicsMaterial", menuName = "TDFP/PhysicsMaterial", order = 1)]
    public class TFPhysicsMaterial : ScriptableObject
    {
        public Fix staticFriction = (Fix).4f;
        public Fix dynamicFriction = (Fix).2f;
        public Fix bounciness;
    }
}