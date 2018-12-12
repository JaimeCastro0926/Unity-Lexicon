// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomEditor(typeof(LexiconRuntime))]
    public class LexiconRuntimeEditor : Editor
    {
        private SerializedProperty workspace;
        private SerializedProperty speechToTextActive;
        private SerializedProperty conversationActive;
        private SerializedProperty speechConfidenceThreshold;
        private SerializedProperty useCustomLanguageModel;
        private SerializedProperty customLanguageModelWeight;
        private SerializedProperty keywordConfidenceThreshold;
        private SerializedProperty keywords;
        private SerializedProperty intentConfidenceThreshold;
        private SerializedProperty matchMultipleIntents;
        private SerializedProperty silenceThreshold;
        private SerializedProperty timestampOffset;
        private SerializedProperty useDwellPosition;

        void OnEnable()
        {
            workspace = serializedObject.FindProperty("workspace");
            speechToTextActive = serializedObject.FindProperty("speechToTextActive");
            conversationActive = serializedObject.FindProperty("conversationActive");
            speechConfidenceThreshold = serializedObject.FindProperty("speechConfidenceThreshold");
            useCustomLanguageModel = serializedObject.FindProperty("useCustomLanguageModel");
            customLanguageModelWeight = serializedObject.FindProperty("customLanguageModelWeight");
            keywordConfidenceThreshold = serializedObject.FindProperty("keywordConfidenceThreshold");
            keywords = serializedObject.FindProperty("keywords");
            intentConfidenceThreshold = serializedObject.FindProperty("intentConfidenceThreshold");
            matchMultipleIntents = serializedObject.FindProperty("matchMultipleIntents");
            silenceThreshold = serializedObject.FindProperty("silenceThreshold");
            timestampOffset = serializedObject.FindProperty("timestampOffset");
            useDwellPosition = serializedObject.FindProperty("useDwellPosition");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            //EditorGUIUtility.labelWidth = 100;

            EditorGUILayout.PropertyField(workspace);
            EditorGUILayout.Space();

            LexiconWorkspace assignedWorkspace = ((LexiconRuntime)target).Workspace;

            if (assignedWorkspace != null)
            {
                EditorGUIUtility.labelWidth = 200;

                if (assignedWorkspace.UseWatsonSpeechToText)
                {
                    EditorGUILayout.PropertyField(speechToTextActive);
                }

                if (assignedWorkspace.UseWatsonConversation)
                {
                    EditorGUILayout.PropertyField(conversationActive);
                }

                EditorGUILayout.Space();

                if (assignedWorkspace.UseWatsonSpeechToText)
                {
                    EditorGUILayout.LabelField("Speech to Text Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(speechConfidenceThreshold);
                    if (assignedWorkspace.WatsonSpeechToTextManager.CreateCustomModel)
                    {
                        EditorGUILayout.PropertyField(useCustomLanguageModel);
                        if (useCustomLanguageModel.boolValue)
                        {
                            EditorGUILayout.PropertyField(customLanguageModelWeight);
                        }
                    }
                    EditorGUILayout.PropertyField(keywordConfidenceThreshold);
                    EditorGUILayout.PropertyField(keywords, true);
                }

                EditorGUILayout.Space();

                if (assignedWorkspace.UseWatsonConversation)
                {
                    EditorGUILayout.LabelField("Conversation Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(intentConfidenceThreshold);
                    EditorGUILayout.PropertyField(matchMultipleIntents);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Calibration", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(silenceThreshold);
                EditorGUILayout.PropertyField(timestampOffset);
                EditorGUILayout.PropertyField(useDwellPosition);
            }

            EditorGUIUtility.labelWidth = 0;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
