// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCompleteSpeechToTextSync : WatsonSyncAction
    {
        public static WatsonCompleteSpeechToTextSync CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCompleteSpeechToTextSync instance = CreateInstance<WatsonCompleteSpeechToTextSync>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            Debug.Log("Watson Sync Speech to Text Completed: " + (succeeded ? "Succeeded" : "Failed"));

            if (succeeded)
            {
                workspace.WatsonSpeechToTextManager.LastSyncTime = DateTime.Now.ToString("g");
                workspace.WatsonSpeechToTextManager.IsSyncing = false;
                EditorUtility.SetDirty(workspace);
                AssetDatabase.SaveAssets();

                EditorManager.Instance.MonitorWatsonSpeechToTextTraining(workspace);
            }
            else
            {
                workspace.WatsonSpeechToTextManager.SyncStatus = "Sync Failed";
                workspace.WatsonSpeechToTextManager.IsSyncing = false;
                EditorUtility.SetDirty(workspace);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

#endif
