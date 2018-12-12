// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class ReloadSyncAction : SyncAction
    {
        public bool triggeredReload;
        public static bool reloaded;

        public static ReloadSyncAction CreateInstance()
        {
            ReloadSyncAction instance = CreateInstance<ReloadSyncAction>();
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            Debug.Log("ReloadSyncAction process");

            if (!triggeredReload)
            {
                Debug.Log("Triggering reload");
                reloaded = false;
                triggeredReload = true;
                AssetDatabase.Refresh();

                //EditorUtility.SetDirty(queue);
                //AssetDatabase.SaveAssets();
            }

            succeeded = reloaded;
            isDone = true;
            retry = true;
            retryDelay = 5.0f;
        }

        [DidReloadScripts]
        public static void DidReload()
        {
            reloaded = true;
        }
    }
}

#endif
