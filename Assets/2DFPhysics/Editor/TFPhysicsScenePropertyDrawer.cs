using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TF.Core.Editor
{
    [CustomPropertyDrawer(typeof(TFPhysicsScene))]
    public class TFPhysicsScenePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var buttonRect = new Rect(position.x, position.y, 80, position.height);

            if(GUI.Button(buttonRect, "View Dynamic Tree"))
            {
                //DynamicTreeViewerEditor.OpenWindow();
            }

            EditorGUI.EndProperty();
        }
    }
}