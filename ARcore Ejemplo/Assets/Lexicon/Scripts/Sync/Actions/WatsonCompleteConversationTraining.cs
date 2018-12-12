// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCompleteConversationTraining : WatsonSyncAction
    {
        public static WatsonCompleteConversationTraining CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCompleteConversationTraining instance = CreateInstance<WatsonCompleteConversationTraining>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            Debug.Log("Watson Train Conversation Complete: " + (succeeded ? "Succeeded" : "Failed"));

            workspace.WatsonConversationManager.IsTraining = false;
            EditorUtility.SetDirty(workspace);
            AssetDatabase.SaveAssets();
        }
    }
}

#endif
