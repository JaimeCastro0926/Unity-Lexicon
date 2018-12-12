// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomEditor(typeof(LexiconPlaceholderAction))]
    public class PlaceholderActionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Placeholder, your script is compiling...", MessageType.Info);
        }
    }
}
