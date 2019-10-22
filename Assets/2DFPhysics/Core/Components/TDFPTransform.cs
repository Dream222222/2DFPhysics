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

        public Mat22 Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                UpdateTransformRotation();
            }
        }

        public FixVec3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                transform.localScale = (Vector3)scale;
            }
        }

        [SerializeField] private FixVec3 position;
        public FixVec3 scale;
        [HideInInspector]public Mat22 rotation;

        public void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }
            transform.position = (Vector3)position;
            transform.localScale = (Vector3)scale;
            UpdateTransformRotation();
        }

        private void UpdateTransformRotation()
        {
            Vector3 ea = transform.eulerAngles;
            ea.z = (float)Mat22.MatrixToDegrees(rotation);
            transform.eulerAngles = ea;
        }
    }
}