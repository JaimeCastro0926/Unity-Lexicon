// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomEditor(typeof(LexiconIntent))]
    public class LexiconIntentEditor : Editor
    {
        private SerializedProperty intentName;
        private SerializedProperty actionName;
        private SerializedProperty namespaceString;
        private SerializedProperty defaultAction;

        private ReorderableList requiredEntityList;
        private ReorderableList optionalEntityList;
        private ReorderableList phraseList;

        private bool changed;

        void OnEnable()
        {
            intentName = serializedObject.FindProperty("intentName");
            actionName = serializedObject.FindProperty("actionName");
            namespaceString = serializedObject.FindProperty("namespaceString");
            defaultAction = serializedObject.FindProperty("defaultAction");

            requiredEntityList = CreateReorderableList("requiredEntities", "Required Entities");
            optionalEntityList = CreateReorderableList("optionalEntities", "Optional Entities");
            phraseList = CreateReorderableList("phrases", "Phrases");

            if (string.IsNullOrEmpty(namespaceString.stringValue))
            {
                string intentNamespace = PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.ActionNamespace);
                if (string.IsNullOrEmpty(intentNamespace))
                {
                    intentNamespace = "LexiconActions";
                }
                namespaceString.stringValue = intentNamespace;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnDisable()
        {
            if (changed)
            {
                if (PlayerPrefs.GetInt(LexiconConstants.PlayerPrefs.AutoGenerateStrings, 1) == 1)
                {
                    EditorManager.Instance.SaveIntent((LexiconIntent)target);
                    changed = false;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.LabelField("Lexicon Intent", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(intentName);
            EditorGUILayout.PropertyField(actionName);
            EditorGUILayout.PropertyField(namespaceString, new GUIContent("Namespace"));
            EditorGUILayout.PropertyField(defaultAction);
            if (defaultAction.objectReferenceValue == null && actionName.stringValue.Length > 0)
            {
                if (GUILayout.Button("Create Action"))
                {
                    EditorManager.Instance.CreateDefaultAction((LexiconIntent)target);
                }
            }
            if (PlayerPrefs.GetInt(LexiconConstants.PlayerPrefs.AutoGenerateStrings, 1) == 0)
            {
                if (GUILayout.Button("Generate Strings"))
                {
                    EditorManager.Instance.SaveIntent((LexiconIntent)target);
                }
            }
            EditorGUILayout.Space();

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            requiredEntityList.DoLayoutList();
            EditorGUILayout.Space();

            optionalEntityList.DoLayoutList();
            EditorGUILayout.Space();

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            phraseList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }
        }

        private ReorderableList CreateReorderableList(string propertyName, string headerName)
        {
            ReorderableList list = new ReorderableList(serializedObject,
                                                       serializedObject.FindProperty(propertyName),
                                                       true, true, true, true);

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty listItem = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(
                    new Rect(rect.x + 2, rect.y + 2, rect.width - 2, EditorGUIUtility.singleLineHeight),
                    listItem, GUIContent.none);
            };

            list.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, headerName);
            };

            return list;
        }
    }
}
