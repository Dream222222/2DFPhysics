using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TDFP.Core.Editor
{
    [CustomPropertyDrawer(typeof(Mat22))]
    public class Mat22PropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var valueRect = new Rect(position.x, position.y, 45, position.height);
            var unitRect = new Rect(position.x + 50, position.y, 45, position.height);
            var buttonRect = new Rect(position.x + 100, position.y, 20, position.height);

            //EditorGUI.LabelField(valueRect, Mat22.MatrixToDegrees(property.serializedObject.targetObject as Mat22).ToString());
            //val = EditorGUI.DoubleField(unitRect, val);
            //if (GUI.Button(buttonRect, "A"))
            //{
            //    property.FindPropertyRelative("raw").longValue = ((FixConst)val).raw;
            //}

            EditorGUI.EndProperty();
        }
    }
}
