// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomEditor(typeof(LexiconEntity), true)]
    public class LexiconEntityEditor : Editor
    {
        private SerializedProperty entityName;

        private ReorderableList valueList;

        private bool changed;

        void OnEnable()
        {
            entityName = serializedObject.FindProperty("entityName");

            if (serializedObject.FindProperty("values") != null)
            {
                valueList = new ReorderableList(serializedObject,
                                                serializedObject.FindProperty("values"),
                                                true, true, true, true);

                valueList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty entityValue = valueList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, entityValue, true);
                };

                valueList.drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "Entity Values");
                };

                valueList.onAddCallback = (list) =>
                {
                    int index = valueList.serializedProperty.arraySize;
                    valueList.serializedProperty.InsertArrayElementAtIndex(index);

                    serializedObject.ApplyModifiedProperties();
                    ((LexiconEntity)target).Values[index].Initialize();
                };
            }
        }

        private void OnDisable()
        {
            if (changed)
            {
                if (PlayerPrefs.GetInt(LexiconConstants.PlayerPrefs.AutoGenerateStrings, 1) == 1)
                {
                    EditorManager.Instance.SaveEntity((LexiconEntity)target);
                    changed = false;
                }
            }

            //Debug.Log("Force Save: " + target.name);
            //AssetDatabase.Refresh();
            //EditorUtility.SetDirty(target);
            //AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.LabelField("Lexicon Entity", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(entityName);
            EditorGUILayout.Space();

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            if (valueList != null)
            {
                valueList.DoLayoutList();
            }

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }
        }
    }
}
