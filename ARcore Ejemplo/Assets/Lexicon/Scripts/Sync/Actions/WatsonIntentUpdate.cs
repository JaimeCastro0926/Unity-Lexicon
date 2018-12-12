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
    public class WatsonIntentUpdate : WatsonSyncAction
    {
        public IntentData localIntent;

        public static WatsonIntentUpdate CreateInstance(LexiconWorkspace workspace, IntentData intentData)
        {
            WatsonIntentUpdate instance = CreateInstance<WatsonIntentUpdate>();
            instance.workspace = workspace;
            instance.localIntent = intentData;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                // TODO: Would be better to update phrases individually instead of replacing all

                conversation.UpdateIntent(HandleSuccessCallback,
                                          HandleFailCallback,
                                          workspace.WatsonConversationManager.WorkspaceId,
                                          localIntent.name,
                                          null,
                                          localIntent.phrases.ToArray());
            }
        }

        void HandleSuccessCallback(Intent watsonIntent, Dictionary<string, object> customData)
        {
            if (watsonIntent != null)
            {
                workspace.WatsonConversationManager.Timestamps.SetIntentTimestamp(localIntent.name, watsonIntent.updated);
                succeeded = true;
            }
            else
            {
                succeeded = false;
            }

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
