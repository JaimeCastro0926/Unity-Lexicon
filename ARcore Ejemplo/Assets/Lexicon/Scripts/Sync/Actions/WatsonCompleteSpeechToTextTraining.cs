// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCompleteSpeechToTextTraining : WatsonSyncAction
    {
        public static WatsonCompleteSpeechToTextTraining CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCompleteSpeechToTextTraining instance = CreateInstance<WatsonCompleteSpeechToTextTraining>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            Debug.Log("Watson Train Speech to Text Completed: " + (succeeded ? "Succeeded" : "Failed"));

            workspace.WatsonSpeechToTextManager.IsTraining = false;
            EditorUtility.SetDirty(workspace);
            AssetDatabase.SaveAssets();
        }
    }
}

#endif
