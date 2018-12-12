// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomPropertyDrawer(typeof(LexiconCustomWord), true)]
    public class CustomWordDrawer : PropertyDrawer
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

            SerializedProperty word = property.FindPropertyRelative("word");
            SerializedProperty displayAs = property.FindPropertyRelative("displayAs");
            SerializedProperty pronunciations = property.FindPropertyRelative("pronunciations");

            float spacing = 4;
            float wordWidth = 100;
            float displayAsWidth = 100;
            float pronunciationsWidth = position.width - wordWidth - displayAsWidth - spacing * 4;

            Rect wordRect = new Rect(position.x + spacing, position.y, wordWidth, EditorGUIUtility.singleLineHeight);
            Rect displayAsRect = new Rect(position.x + wordWidth + spacing * 2, position.y, displayAsWidth, EditorGUIUtility.singleLineHeight);
            Rect pronunciationsRect = new Rect(position.x + wordWidth + displayAsWidth + spacing * 3, position.y, pronunciationsWidth, EditorGUIUtility.singleLineHeight);

            string wordHint = "word";
            string displayAsHint = "display as";
            string pronunciationsHint = "pronunciations";
            string temp = "";

            if (word.stringValue.Length == 0)
            {
                temp = EditorGUI.TextField(wordRect, wordHint, textFieldHintStyle);
                if (temp != wordHint)
                {
                    word.stringValue = temp;
                }
            }
            else
            {
                word.stringValue = EditorGUI.TextField(wordRect, word.stringValue);
            }

            if (displayAs.stringValue.Length == 0 || string.Equals(word.stringValue, displayAs.stringValue))
            {
                temp = EditorGUI.TextField(displayAsRect, displayAsHint, textFieldHintStyle);
                if (temp != displayAsHint)
                {
                    displayAs.stringValue = temp;
                }
            }
            else
            {
                displayAs.stringValue = EditorGUI.TextField(displayAsRect, displayAs.stringValue);
            }

            if (pronunciations.stringValue.Length == 0 || string.Equals(word.stringValue, pronunciations.stringValue))
            {
                temp = EditorGUI.TextField(pronunciationsRect, pronunciationsHint, textFieldHintStyle);
                if (temp != pronunciationsHint)
                {
                    pronunciations.stringValue = temp;
                }
            }
            else
            {
                pronunciations.stringValue = EditorGUI.TextField(pronunciationsRect, pronunciations.stringValue);
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
