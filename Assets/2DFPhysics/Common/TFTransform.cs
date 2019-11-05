using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedPointy;

namespace TF.Core
{
    public class TFTransform : MonoBehaviour
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

        public FixVec3 LocalScale
        {
            get
            {
                return localScale;
            }
            set
            {
                localScale = value;
                transform.localScale = (Vector3)localScale;
            }
        }

        [SerializeField] private FixVec3 position;
        [SerializeField] private FixVec3 localScale;
        [HideInInspector] private Mat22 rotation;

        public void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }
            transform.position = (Vector3)position;
            transform.localScale = (Vector3)localScale;
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