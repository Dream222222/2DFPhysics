using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;

namespace TDFP.Core
{
    public class TDFPTransform : MonoBehaviour
    {
        public FixVec3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                transform.position = (Vector3)position;
            }
        }

        [SerializeField] private FixVec3 position;
        public FixQuaternion rotation;
        public FixVec3 scale;

    }
}