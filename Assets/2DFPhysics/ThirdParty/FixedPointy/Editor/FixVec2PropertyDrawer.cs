using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixedPointy;

[CustomPropertyDrawer(typeof(FixVec2))]
public class FixVec2PropertyDrawer : PropertyDrawer
{
    double valX, valY;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var valueLabelRect = new Rect(position.x, position.y, 50, position.height);
        var valueXRect = new Rect(position.x + 65, position.y, 50, position.height);
        var valueYRect = new Rect(position.x + 115, position.y, 50, position.height);
        var buttonRect = new Rect(position.x + 175, position.y, 20, position.height);

        EditorGUI.LabelField(valueLabelRect, $"{Fix.RawToString(property.FindPropertyRelative("_x").FindPropertyRelative("raw").intValue)},{Fix.RawToString(property.FindPropertyRelative("_y").FindPropertyRelative("raw").intValue)}");

        valX = EditorGUI.DoubleField(valueXRect, valX);
        valY = EditorGUI.DoubleField(valueYRect, valY);

        if (GUI.Button(buttonRect, "A"))
        {
            property.FindPropertyRelative("_x").FindPropertyRelative("raw").intValue = Fix.DoubleToRaw(valX);
            property.FindPropertyRelative("_y").FindPropertyRelative("raw").intValue = Fix.DoubleToRaw(valY);
        }

        EditorGUI.EndProperty();
    }
}