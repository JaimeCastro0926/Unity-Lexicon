﻿// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class SettingsWindow : EditorWindow
    {
        private string actionNamespace;
        private bool autoGenerateStrings = true;

        [MenuItem("Lexicon/Settings", false, 3)]
        static void Init()
        {
            SettingsWindow window = (SettingsWindow)GetWindow(typeof(SettingsWindow), true, "Lexicon Settings");
            window.Show();
        }

        void OnFocus()
        {
            if (PlayerPrefs.HasKey(LexiconConstants.PlayerPrefs.ActionNamespace))
            {
                actionNamespace = PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.ActionNamespace);
            }

            if (PlayerPrefs.HasKey(LexiconConstants.PlayerPrefs.AutoGenerateStrings))
            {
                autoGenerateStrings = PlayerPrefs.GetInt(LexiconConstants.PlayerPrefs.AutoGenerateStrings) == 1;
            }
        }

        private void OnLostFocus()
        {
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.ActionNamespace, actionNamespace);
            PlayerPrefs.SetInt(LexiconConstants.PlayerPrefs.AutoGenerateStrings, autoGenerateStrings ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.ActionNamespace, actionNamespace);
            PlayerPrefs.SetInt(LexiconConstants.PlayerPrefs.AutoGenerateStrings, autoGenerateStrings ? 1 : 0);
            PlayerPrefs.Save();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Changing the Company Name or Product Name for this project will reset these settings.", MessageType.Info);

            EditorGUILayout.Space();

            actionNamespace = EditorGUILayout.TextField(new GUIContent("Action Namespace", "This namespace will be used for the generated Intent action and string classes."), actionNamespace);

            autoGenerateStrings = EditorGUILayout.Toggle(new GUIContent("Auto Generate Strings", "If checked the Intent string files will be autogenerated whenever an Intent or associated Entity changes."), autoGenerateStrings);
        }
    }
}
