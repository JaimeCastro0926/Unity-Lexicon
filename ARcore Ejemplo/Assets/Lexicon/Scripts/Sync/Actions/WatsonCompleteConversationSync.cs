// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCompleteConversationSync : WatsonSyncAction
    {
        public static WatsonCompleteConversationSync CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCompleteConversationSync instance = CreateInstance<WatsonCompleteConversationSync>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            Debug.Log("Watson Sync Conversation Complete: " + (succeeded ? "Succeeded" : "Failed"));

            if (succeeded)
            {
                workspace.WatsonConversationManager.LastSyncTime = DateTime.Now.ToString("g");
                workspace.WatsonConversationManager.LastSyncData = ((WatsonSyncQueue)queue).syncData;
                workspace.WatsonConversationManager.IsSyncing = false;
                EditorUtility.SetDirty(workspace);
                AssetDatabase.SaveAssets();

                EditorManager.Instance.MonitorWatsonConversationTraining(workspace);
            }
            else
            {
                workspace.WatsonConversationManager.SyncStatus = "Sync Failed";
                workspace.WatsonConversationManager.IsSyncing = false;
                EditorUtility.SetDirty(workspace);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

#endif
