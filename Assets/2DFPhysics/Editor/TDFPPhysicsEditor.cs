using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixedPointy;

namespace TDFP.Core.Editor
{
    [CustomEditor(typeof(TDFPhysics))]
    public class TDFPPhysicsEditor : UnityEditor.Editor
    {
        TDFPhysics t;
        float rotVal;

        public override void OnInspectorGUI()
        {
            t = (TDFPhysics)target;

            DrawDefaultInspector();

            if (GUILayout.Button("View Dynamic Tree", GUILayout.Width(150)))
            {
                DynamicTreeViewerEditor.OpenWindow(TDFPhysics.physicsScene);
            }
        }
    }
}