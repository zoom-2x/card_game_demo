using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using CardGame.Attributes;

[CustomPropertyDrawer(typeof(EnumNamedArrayAttribute))]
public class EnumNamedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnumNamedArrayAttribute names = attribute as EnumNamedArrayAttribute;

        int index = System.Convert.ToInt32(property.propertyPath.Substring(property.propertyPath.IndexOf("["))
                        .Replace("[", "")
                        .Replace("]", ""));

        label.text = names.names[index];
        EditorGUI.PropertyField(position, property, label, true);
    }
}
