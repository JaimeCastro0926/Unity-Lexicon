// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using Mixspace.Lexicon.Watson.Conversation.v1;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonIntentSyncAll : WatsonSyncAction
    {
        [NonSerialized]
        private WatsonSyncQueue queue;

        public static WatsonIntentSyncAll CreateInstance(LexiconWorkspace workspace)
        {
            WatsonIntentSyncAll instance = CreateInstance<WatsonIntentSyncAll>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                this.queue = (WatsonSyncQueue)queue;

                conversation.ListIntents(HandleSuccessCallback,
                                         HandleFailCallback,
                                         workspace.WatsonConversationManager.WorkspaceId,
                                         false);
            }
        }

        void HandleSuccessCallback(ListIntentsResp intentsResp, Dictionary<string, object> customData)
        {
            if (intentsResp == null)
            {
                succeeded = false;
                isDone = true;
                return;
            }

            WorkspaceSyncData syncData = queue.syncData;

            HashSet<string> watsonIntentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Intent watsonIntent in intentsResp.intents)
            {
                string intentName = watsonIntent.intent;
                IntentData localIntent;

                if (syncData.intentData.TryGetValue(intentName, out localIntent))
                {
                    string localTimestamp = workspace.WatsonConversationManager.Timestamps.GetIntentTimestamp(intentName);
                    bool needsUpdate = true;

                    if (watsonIntent.updated.Equals(localTimestamp, StringComparison.OrdinalIgnoreCase))
                    {
                        // No remote changes

                        if (workspace.WatsonConversationManager.LastSyncData != null)
                        {
                            IntentData lastSync;
                            if (workspace.WatsonConversationManager.LastSyncData.intentData.TryGetValue(intentName, out lastSync))
                            {
                                if (WorkspaceSyncData.CompareIntentData(localIntent, lastSync))
                                {
                                    // No local changes
                                    needsUpdate = false;
                                }
                            }
                        }
                    }

                    if (needsUpdate)
                    {
                        queue.Enqueue(WatsonIntentUpdate.CreateInstance(workspace, localIntent));
                    }
                }
                else
                {
                    // Intent deleted locally
                    queue.Enqueue(WatsonIntentDelete.CreateInstance(workspace, intentName));
                }

                watsonIntentNames.Add(intentName);
            }

            foreach (IntentData localIntent in syncData.intentData.Values)
            {
                if (!watsonIntentNames.Contains(localIntent.name))
                {
                    // Intent does not exist on server
                    queue.Enqueue(WatsonIntentCreate.CreateInstance(workspace, localIntent));
                }
            }

            succeeded = true;
            isDone = true;
        }

        void HandleFailCallback(RESTConnector.Error error, Dictionary<string, object> customData)
        {
            Debug.LogError(error);

            succeeded = false;
            isDone = true;
        }
    }
}

#endif
