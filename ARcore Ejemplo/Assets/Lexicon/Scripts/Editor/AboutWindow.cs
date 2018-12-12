// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class AboutWindow : EditorWindow
    {
        [MenuItem("Lexicon/About", false, 1)]
        static void Init()
        {
            AboutWindow window = (AboutWindow)GetWindow(typeof(AboutWindow), true, "About Lexicon");
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Mixspace Lexicon");

            EditorGUILayout.Space();

            GUILayout.Label("(c) 2018 Mixspace Technologies, LLC");
            GUILayout.Label("All Rights Reserved.");

            EditorGUILayout.Space();

            GUILayout.Label("Version " + LexiconConstants.Version);
        }
    }
}
