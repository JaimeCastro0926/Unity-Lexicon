// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomEditor(typeof(LexiconWorkspace))]
    public class LexiconWorkspaceEditor : Editor
    {
        private SerializedProperty workspaceName;
        private SerializedProperty workspaceDescription;
        private SerializedProperty useWatsonSpeechToText;
        private SerializedProperty watsonSpeechToTextManager;
        private SerializedProperty watsonSpeechToTextURL;
        private SerializedProperty watsonSpeechToTextModel;
        private SerializedProperty watsonSpeechToTextCreateCustomModel;
        private SerializedProperty watsonSpeechToTextCustomizationId;
        private SerializedProperty useWatsonConversation;
        private SerializedProperty watsonConversationManager;
        private SerializedProperty watsonConversationURL;
        private SerializedProperty watsonConversationLanguage;
        private SerializedProperty watsonConversationId;

        private ReorderableList intentList;
        private ReorderableList customWordList;

        private bool showWatsonSpeechToTextSettings = false;
        private bool showWatsonSpeechToTextCustomWords = false;
        private bool showWatsonConversationSettings = false;
        private bool watsonSpeechToTextIdLocked = true;
        private bool watsonConversationIdLocked = true;

        private bool changed;

        void OnEnable()
        {
            workspaceName = serializedObject.FindProperty("workspaceName");
            workspaceDescription = serializedObject.FindProperty("workspaceDescription");
            useWatsonSpeechToText = serializedObject.FindProperty("useWatsonSpeechToText");
            watsonSpeechToTextManager = serializedObject.FindProperty("watsonSpeechToTextManager");
            watsonSpeechToTextURL = watsonSpeechToTextManager.FindPropertyRelative("url");
            watsonSpeechToTextModel = watsonSpeechToTextManager.FindPropertyRelative("model");
            watsonSpeechToTextCreateCustomModel = watsonSpeechToTextManager.FindPropertyRelative("createCustomModel");
            watsonSpeechToTextCustomizationId = watsonSpeechToTextManager.FindPropertyRelative("customizationId");
            useWatsonConversation = serializedObject.FindProperty("useWatsonConversation");
            watsonConversationManager = serializedObject.FindProperty("watsonConversationManager");
            watsonConversationURL = watsonConversationManager.FindPropertyRelative("url");
            watsonConversationLanguage = watsonConversationManager.FindPropertyRelative("language");
            watsonConversationId = watsonConversationManager.FindPropertyRelative("workspaceId");

            intentList = CreateIntentList();
            customWordList = CreateCustomWordList();
        }

        private void OnDisable()
        {
            if (changed)
            {
                // TODO: Revisit this.
                //RemoveNullValues();
                //EditorManager.Instance.SaveWorkspace((Workspace)target);
            }
        }

        private void RemoveNullValues()
        {
            serializedObject.Update();

            SerializedProperty intents = serializedObject.FindProperty("intents");

            for (int i = intents.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty intent = intents.GetArrayElementAtIndex(i);
                if (intent.objectReferenceValue == null)
                {
                    intents.DeleteArrayElementAtIndex(i);
                }
            }

            serializedObject.ApplyModifiedProperties();

            /*
            Workspace workspace = (Workspace)target;
            for (int i = workspace.intents.Count - 1; i >= 0; i--)
            {
                if (workspace.intents[i] == null)
                {
                    Debug.Log("removing null intent");
                    workspace.intents.RemoveAt(i);
                }
            }
            */
        }

        public override void OnInspectorGUI()
        {
            //Debug.Log("Inspector width: " + Screen.width);

            LexiconWorkspace workspace = (LexiconWorkspace)target;

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUIUtility.labelWidth = 160;

            EditorGUILayout.LabelField("Lexicon Workspace", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(workspaceName);
            EditorGUILayout.PropertyField(workspaceDescription);
            EditorGUILayout.Space();

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(useWatsonSpeechToText);
            if (useWatsonSpeechToText.boolValue)
            {
                EditorGUI.indentLevel++;

                showWatsonSpeechToTextSettings = EditorGUILayout.Foldout(showWatsonSpeechToTextSettings, "Settings", true);
                if (showWatsonSpeechToTextSettings)
                {
                    EditorGUILayout.PropertyField(watsonSpeechToTextURL, new GUIContent("Speech to Text URL"));
                    EditorGUILayout.PropertyField(watsonSpeechToTextModel, new GUIContent("Base Language Model"));
                    EditorGUILayout.PropertyField(watsonSpeechToTextCreateCustomModel, new GUIContent("Create Custom Model"));
                    if (watsonSpeechToTextCreateCustomModel.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = !watsonSpeechToTextIdLocked;
                        EditorGUILayout.PropertyField(watsonSpeechToTextCustomizationId);
                        GUI.enabled = true;
                        watsonSpeechToTextIdLocked = EditorGUILayout.Toggle(watsonSpeechToTextIdLocked, "IN LockButton", new GUILayoutOption[] { GUILayout.MaxWidth(20) });
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (workspace.CustomWords != null && workspace.CustomWords.Count > 0)
                {
                    showWatsonSpeechToTextCustomWords = EditorGUILayout.Foldout(showWatsonSpeechToTextCustomWords, "Custom Words", true);
                    if (showWatsonSpeechToTextCustomWords)
                    {
                        customWordList.DoLayoutList();
                    }
                }

                EditorGUI.indentLevel--;

                if (watsonSpeechToTextCreateCustomModel.boolValue)
                {
                    EditorGUILayout.LabelField("Status: " + workspace.WatsonSpeechToTextManager.SyncStatus);
                    if (!string.IsNullOrEmpty(workspace.WatsonSpeechToTextManager.LastSyncTime))
                    {
                        EditorGUILayout.LabelField("Last Sync: " + workspace.WatsonSpeechToTextManager.LastSyncTime);
                    }
                }

                if (watsonSpeechToTextCreateCustomModel.boolValue)
                {
                    if (workspace.WatsonSpeechToTextManager.IsSyncing)
                    {
                        if (GUILayout.Button("Cancel Speech to Text Sync"))
                        {
                            EditorManager.Instance.CancelWatsonSpeechToTextSync(workspace);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Sync Watson Speech to Text"))
                        {
                            EditorManager.Instance.SyncWatsonSpeechToText(workspace);
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(useWatsonConversation);
            if (useWatsonConversation.boolValue)
            {
                EditorGUI.indentLevel++;

                showWatsonConversationSettings = EditorGUILayout.Foldout(showWatsonConversationSettings, "Settings", true);
                if (showWatsonConversationSettings)
                {
                    EditorGUILayout.PropertyField(watsonConversationURL, new GUIContent("Workspace URL"));
                    EditorGUILayout.PropertyField(watsonConversationLanguage, new GUIContent("Workspace Language"));
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = !watsonConversationIdLocked;
                    EditorGUILayout.PropertyField(watsonConversationId, new GUIContent("Workspace Id"));
                    GUI.enabled = true;
                    watsonConversationIdLocked = EditorGUILayout.Toggle(watsonConversationIdLocked, "IN LockButton", new GUILayoutOption[] { GUILayout.MaxWidth(20) });
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Status: " + workspace.WatsonConversationManager.SyncStatus);
                if (!string.IsNullOrEmpty(workspace.WatsonConversationManager.LastSyncTime))
                {
                    EditorGUILayout.LabelField("Last Sync: " + workspace.WatsonConversationManager.LastSyncTime);
                }

                if (workspace.WatsonConversationManager.IsSyncing)
                {
                    if (GUILayout.Button("Cancel Conversation Sync"))
                    {
                        EditorManager.Instance.CancelWatsonConversationSync(workspace);
                    }
                }
                else
                {
                    if (GUILayout.Button("Sync Watson Conversation"))
                    {
                        EditorManager.Instance.SyncWatsonConversation(workspace);
                    }
                }
            }

            EditorGUILayout.Space();

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.grey);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            intentList.DoLayoutList();

            EditorGUIUtility.labelWidth = 0;

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }
        }

        private ReorderableList CreateIntentList()
        {
            ReorderableList list = new ReorderableList(serializedObject,
                                           serializedObject.FindProperty("intents"),
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
                EditorGUI.LabelField(rect, "Intents");
            };

            return list;
        }

        private ReorderableList CreateCustomWordList()
        {
            ReorderableList list = new ReorderableList(serializedObject,
                                           serializedObject.FindProperty("customWords"),
                                           false, false, false, false);

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty listItem = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(
                    new Rect(rect.x + 2, rect.y + 2, rect.width - 2, EditorGUIUtility.singleLineHeight),
                    listItem, GUIContent.none);
            };

            return list;
        }
    }
}
