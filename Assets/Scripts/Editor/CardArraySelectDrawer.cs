using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using CardGame.Attributes;
using CardGame.Data;

[CustomPropertyDrawer(typeof(CardArraySelectAttribute))]
public class CardArraySelectDrawer : PropertyDrawer
{
    int index = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CardArraySelectAttribute attr = (CardArraySelectAttribute) attribute;
        string[] list = CardLibrary.CARD_ATTRIBUTES[attr.selection];

        index = EditorGUI.Popup(
                new Rect(0, 0, position.width, 20),
                attr.label,
                property.intValue,
                list);

        property.intValue = index;
    }
}
