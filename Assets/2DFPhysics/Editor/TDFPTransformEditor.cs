using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixedPointy;

namespace TDFP.Core.Editor
{
    [CustomEditor(typeof(TDFPTransform))]
    public class TDFPTransformEditor : UnityEditor.Editor
    {
        TDFPTransform t;
        float rotVal;

        public override void OnInspectorGUI()
        {
            t = (TDFPTransform)target;

            DrawDefaultInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rotation: ", GUILayout.Width(75));
            EditorGUILayout.LabelField(Mat22.MatrixToDegrees(t.rotation).ToString(), GUILayout.Width(50));
            rotVal = EditorGUILayout.FloatField(rotVal, GUILayout.Width(50));
            if (GUILayout.Button("A", GUILayout.Width(25)))
            {
                t.rotation = new Mat22((Fix)rotVal);
                t.OnValidate();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}