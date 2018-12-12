// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomPropertyDrawer(typeof(LexiconEntityValue), true)]
    public class EntityValueDrawer : PropertyDrawer
    {
        private static GUIStyle textFieldHintStyle = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (textFieldHintStyle == null)
            {
                textFieldHintStyle = new GUIStyle(EditorStyles.textField);
                textFieldHintStyle.fontStyle = FontStyle.Italic;
                textFieldHintStyle.fontSize = 9;
                textFieldHintStyle.normal.textColor = Color.grey;
                textFieldHintStyle.padding = new RectOffset(3, 0, 2, 0);
            }

            EditorGUI.BeginProperty(position, label, property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty valueName = property.FindPropertyRelative("valueName");
            SerializedProperty synonyms = property.FindPropertyRelative("synonyms");
            SerializedProperty binding = property.FindPropertyRelative("binding");

            float spacing = 4;
            float bindingWidth = (binding != null) ? 100 : 0;
            float nameWidth = (binding != null) ? 80 : 100;
            float synonymsWidth = position.width - nameWidth - bindingWidth - spacing * 4;

            Rect nameRect = new Rect(position.x + spacing, position.y + 2, nameWidth, EditorGUIUtility.singleLineHeight);
            Rect synonymsRect = new Rect(position.x + nameWidth + spacing * 2, position.y + 2, synonymsWidth, EditorGUIUtility.singleLineHeight);
            Rect bindingRect = new Rect(position.x + nameWidth + synonymsWidth + spacing * 3, position.y + 2, bindingWidth, EditorGUIUtility.singleLineHeight);

            string nameHint = "name";
            string synonymsHint = "synonyms";
            string temp = "";

            if (valueName == null)
                Debug.Log("valueName is null");

            if (valueName.stringValue.Length == 0)
            {
                temp = EditorGUI.TextField(nameRect, nameHint, textFieldHintStyle);
                if (temp != nameHint)
                {
                    valueName.stringValue = temp;
                }
            }
            else
            {
                valueName.stringValue = EditorGUI.TextField(nameRect, valueName.stringValue);
            }

            if (synonyms.stringValue.Length == 0)
            {
                temp = EditorGUI.TextField(synonymsRect, synonymsHint, textFieldHintStyle);
                if (temp != synonymsHint)
                {
                    synonyms.stringValue = temp;
                }
            }
            else
            {
                synonyms.stringValue = EditorGUI.TextField(synonymsRect, synonyms.stringValue);
            }

            if (binding != null)
            {
                EditorGUI.PropertyField(bindingRect, binding, GUIContent.none);
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
