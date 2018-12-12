// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [ExecuteInEditMode]
    public class EditorManager : MonoBehaviour
    {
        public static EditorManager Instance { get { return Singleton<EditorManager>.Instance; } }

        public List<SyncQueue> generateAssetsQueues = new List<SyncQueue>();

        public List<WatsonSyncQueue> watsonSpeechToTextSyncQueues = new List<WatsonSyncQueue>();

        public List<WatsonSyncQueue> watsonConversationSyncQueues = new List<WatsonSyncQueue>();

        [NonSerialized]
        private bool started;

        [NonSerialized]
        private bool assemblyReload;

        [SerializeField]
        private LexiconEntity entityToSave;

        public void OnEnable()
        {
            //Debug.Log("EditorManager starting up");

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            LogSystem.InstallDefaultReactors();
            Runnable.EnableRunnableInEditor();

            EditorManager.Instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
            Runnable.Instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }          public void Update()         {
            // Originally we were calling Process from OnEnable, but this meant any coroutines
            // spawned within Process wouldn't run. This seems to be an issue for any MonoBehaviour
            // persisted with HideFlags.DontSave.

            if (!started)             {                 Process();                 started = true;             }         }

        public void OnDisable()
        {
            //Debug.Log("EditorManager saving state");

            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

            CleanUp();
        }

        public void OnBeforeAssemblyReload()
        {
            //Debug.Log("Before Assembly Reload");
            //assemblyReload = true;
        }

        public void OnAfterAssemblyReload()
        {
            //Debug.Log("After Assembly Reload");
        }

        public void Process()
        {
            //Debug.Log("Process, generateAssetsQueues count: " + generateAssetsQueues.Count);
            //Debug.Log("Process, watsonSpeechToTextSyncQueues count: " + watsonSpeechToTextSyncQueues.Count);
            //Debug.Log("Process, watsonConversationSyncQueues count: " + watsonConversationSyncQueues.Count);

            CleanUp();

            if (entityToSave)
            {
                //Debug.Log("have a stored entity to save...");
                SaveEntity(entityToSave);
                entityToSave = null;
            }

            foreach (SyncQueue queue in generateAssetsQueues)
            {
                queue.Process();
            }

            foreach (WatsonSyncQueue queue in watsonSpeechToTextSyncQueues)
            {
                queue.Process();
            }

            foreach (WatsonSyncQueue queue in watsonConversationSyncQueues)
            {
                queue.Process();
            }
        }

        public void CleanUp()
        {
            //Debug.Log("CleanUp");

            for (int i = generateAssetsQueues.Count - 1; i >= 0; i--)
            {
                SyncQueue queue = generateAssetsQueues[i];
                if (queue.IsFinished)
                {
                    generateAssetsQueues.RemoveAt(i);
                    DestroyImmediate(queue);
                }
            }

            for (int i = watsonSpeechToTextSyncQueues.Count - 1; i >= 0; i--)
            {
                SyncQueue queue = watsonSpeechToTextSyncQueues[i];
                if (queue.IsFinished)
                {
                    watsonSpeechToTextSyncQueues.RemoveAt(i);
                    DestroyImmediate(queue);
                }
            }

            for (int i = watsonConversationSyncQueues.Count - 1; i >= 0; i--)
            {
                SyncQueue queue = watsonConversationSyncQueues[i];
                if (queue.IsFinished)
                {
                    watsonConversationSyncQueues.RemoveAt(i);
                    DestroyImmediate(queue);
                }
            }
        }

        public void SaveIntent(LexiconIntent intent)
        {
            //Debug.Log("SaveIntent");

            SyncQueue assetsQueue = ScriptableObject.CreateInstance<SyncQueue>();

            assetsQueue.Enqueue(ValidateIntent.CreateInstance(intent));
            assetsQueue.Enqueue(GenerateIntentStrings.CreateInstance(intent));
            assetsQueue.Process();

            generateAssetsQueues.Add(assetsQueue);
        }

        public void SaveEntity(LexiconEntity entity)
        {
            //Debug.Log("SaveEntity");

            if (assemblyReload)
            {
                //Debug.Log("Trying to save entity on assembly reload, this is not safe!");
                entityToSave = entity;
            }
            else
            {
                SyncQueue assetsQueue = ScriptableObject.CreateInstance<SyncQueue>();

                assetsQueue.Enqueue(ValidateEntity.CreateInstance(entity));

                List<LexiconIntent> intents = GetAllIntents();

                foreach (LexiconIntent intent in intents)
                {
                    if (intent.UsesEntity(entity))
                    {
                        //Debug.Log("  Generate strings for " + intent.intentName);
                        assetsQueue.Enqueue(GenerateIntentStrings.CreateInstance(intent));
                    }
                }

                assetsQueue.Process();

                generateAssetsQueues.Add(assetsQueue);
            }
        }

        public void CreateDefaultAction(LexiconIntent intent)
        {
            //Debug.Log("CreateAction");

            SyncQueue assetsQueue = ScriptableObject.CreateInstance<SyncQueue>();

            assetsQueue.Enqueue(GenerateActionPrefab.CreateInstance(intent));
            assetsQueue.Enqueue(GenerateIntentStrings.CreateInstance(intent));
            assetsQueue.Enqueue(GenerateActionScript.CreateInstance(intent));
            assetsQueue.Enqueue(ReloadSyncAction.CreateInstance());
            assetsQueue.Enqueue(UpdateActionPrefab.CreateInstance(intent));

            assetsQueue.Process();

            generateAssetsQueues.Add(assetsQueue);
        }

        public void SyncWatson(LexiconWorkspace workspace)
        {
            WorkspaceSyncData syncData = WorkspaceSyncData.Create(workspace);

            if (workspace.UseWatsonSpeechToText)
            {
                SyncWatsonSpeechToText(workspace, syncData);
            }

            if (workspace.UseWatsonConversation)
            {
                SyncWatsonConversation(workspace, syncData);
            }
        }

        public void SyncWatsonSpeechToText(LexiconWorkspace workspace)
        {
            WorkspaceSyncData syncData = WorkspaceSyncData.Create(workspace);

            SyncWatsonSpeechToText(workspace, syncData);
        }

        public void SyncWatsonConversation(LexiconWorkspace workspace)
        {
            WorkspaceSyncData syncData = WorkspaceSyncData.Create(workspace);

            SyncWatsonConversation(workspace, syncData);
        }

        public void SyncWatsonSpeechToText(LexiconWorkspace workspace, WorkspaceSyncData syncData)
        {
            workspace.WatsonSpeechToTextManager.IsSyncing = true;
            workspace.WatsonSpeechToTextManager.SyncStatus = "Syncing";

            WatsonSyncQueue syncQueue = ScriptableObject.CreateInstance<WatsonSyncQueue>();
            syncQueue.workspace = workspace;
            syncQueue.syncData = syncData;

            if (string.IsNullOrEmpty(workspace.WatsonSpeechToTextManager.CustomizationId))
            {
                syncQueue.Enqueue(WatsonCustomModelCreate.CreateInstance(workspace));
            }
            else
            {
                syncQueue.Enqueue(WatsonCustomWordsUpdate.CreateInstance(workspace));
                syncQueue.Enqueue(TimerSyncAction.CreateInstance(10));
            }

            syncQueue.Enqueue(WatsonCorpusAddIntents.CreateInstance(workspace));
            syncQueue.Enqueue(TimerSyncAction.CreateInstance(10));
            syncQueue.Enqueue(WatsonCorpusAddEntities.CreateInstance(workspace));
            syncQueue.Enqueue(TimerSyncAction.CreateInstance(10));
            syncQueue.Enqueue(WatsonCustomModelTrain.CreateInstance(workspace));
            syncQueue.Enqueue(TimerSyncAction.CreateInstance(10));

            syncQueue.OnCompleteAction = WatsonCompleteSpeechToTextSync.CreateInstance(workspace);
            syncQueue.OnQueueFinished += CleanUp;

            syncQueue.Process();

            watsonSpeechToTextSyncQueues.Add(syncQueue);
        }

        public void SyncWatsonConversation(LexiconWorkspace workspace, WorkspaceSyncData syncData)
        {
            workspace.WatsonConversationManager.IsSyncing = true;
            workspace.WatsonConversationManager.SyncStatus = "Syncing";

            WatsonSyncQueue syncQueue = ScriptableObject.CreateInstance<WatsonSyncQueue>();
            syncQueue.workspace = workspace;
            syncQueue.syncData = syncData;

            if (string.IsNullOrEmpty(workspace.WatsonConversationManager.WorkspaceId))
            {
                syncQueue.Enqueue(WatsonWorkspaceCreate.CreateInstance(workspace));
            }

            syncQueue.Enqueue(WatsonIntentSyncAll.CreateInstance(workspace));
            syncQueue.Enqueue(WatsonEntitySyncAll.CreateInstance(workspace));

            syncQueue.OnCompleteAction = WatsonCompleteConversationSync.CreateInstance(workspace);
            syncQueue.OnQueueFinished += CleanUp;

            syncQueue.Process();

            watsonConversationSyncQueues.Add(syncQueue);
        }

        public void MonitorWatsonSpeechToTextTraining(LexiconWorkspace workspace)
        {
            workspace.WatsonSpeechToTextManager.IsTraining = true;

            WatsonSyncQueue syncQueue = ScriptableObject.CreateInstance<WatsonSyncQueue>();
            syncQueue.workspace = workspace;

            syncQueue.Enqueue(WatsonCustomModelStatus.CreateInstance(workspace));
            syncQueue.Enqueue(WatsonCustomWordsGet.CreateInstance(workspace));

            syncQueue.OnCompleteAction = WatsonCompleteSpeechToTextTraining.CreateInstance(workspace);
            syncQueue.OnQueueFinished += CleanUp;

            syncQueue.Process();

            watsonSpeechToTextSyncQueues.Add(syncQueue);
        }

        public void MonitorWatsonConversationTraining(LexiconWorkspace workspace)
        {
            workspace.WatsonConversationManager.IsTraining = true;

            WatsonSyncQueue syncQueue = ScriptableObject.CreateInstance<WatsonSyncQueue>();
            syncQueue.workspace = workspace;

            syncQueue.Enqueue(WatsonWorkspaceStatus.CreateInstance(workspace));

            syncQueue.OnCompleteAction = WatsonCompleteConversationTraining.CreateInstance(workspace);
            syncQueue.OnQueueFinished += CleanUp;

            syncQueue.Process();

            watsonConversationSyncQueues.Add(syncQueue);
        }

        public void CancelWatsonSpeechToTextSync(LexiconWorkspace workspace)
        {
            foreach (WatsonSyncQueue syncQueue in watsonSpeechToTextSyncQueues)
            {
                if (syncQueue.workspace == workspace)
                {
                    syncQueue.Cancel();
                }
            }

            workspace.WatsonSpeechToTextManager.SyncStatus = "Sync Cancelled";
            workspace.WatsonSpeechToTextManager.IsSyncing = false;

            CleanUp();
        }

        public void CancelWatsonConversationSync(LexiconWorkspace workspace)
        {
            foreach (WatsonSyncQueue syncQueue in watsonConversationSyncQueues)
            {
                if (syncQueue.workspace == workspace)
                {
                    syncQueue.Cancel();
                }
            }

            workspace.WatsonConversationManager.SyncStatus = "Sync Cancelled";
            workspace.WatsonConversationManager.IsSyncing = false;

            CleanUp();
        }

        private List<LexiconWorkspace> GetAllWorkspaces()
        {
            List<LexiconWorkspace> workspaces = new List<LexiconWorkspace>();
            string[] guids = AssetDatabase.FindAssets("t:Mixspace.Lexicon.LexiconWorkspace");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LexiconWorkspace workspace = AssetDatabase.LoadAssetAtPath<LexiconWorkspace>(path);
                workspaces.Add(workspace);
            }

            return workspaces;
        }

        private List<LexiconIntent> GetAllIntents()
        {
            List<LexiconIntent> intents = new List<LexiconIntent>();
            string[] guids = AssetDatabase.FindAssets("t:Mixspace.Lexicon.LexiconIntent");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LexiconIntent intent = AssetDatabase.LoadAssetAtPath<LexiconIntent>(path);
                intents.Add(intent);
            }

            return intents;
        }
    }
}

#endif
