using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixedPointy;

namespace TF.Core.Editor
{
    [CustomEditor(typeof(TFPhysics))]
    public class TFPhysicsEditor : UnityEditor.Editor
    {
        TFPhysics t;
        float rotVal;

        public override void OnInspectorGUI()
        {
            t = (TFPhysics)target;

            DrawDefaultInspector();

            if (GUILayout.Button("View Dynamic Tree", GUILayout.Width(150)))
            {
                DynamicTreeViewerEditor.OpenWindow(TFPhysics.physicsScene);
            }
        }
    }
}