// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CustomEditor(typeof(LexiconTextInput), true)]
    public class LexiconTextInputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Process Input"))
                {
                    ((LexiconTextInput)target).ProcessInput();
                }
            }
        }
    }
}
