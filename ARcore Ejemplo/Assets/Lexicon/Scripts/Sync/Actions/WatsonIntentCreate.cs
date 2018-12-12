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
    public class WatsonIntentCreate : WatsonSyncAction
    {
        public IntentData localIntent;

        public static WatsonIntentCreate CreateInstance(LexiconWorkspace workspace, IntentData intentData)
        {
            WatsonIntentCreate instance = CreateInstance<WatsonIntentCreate>();
            instance.workspace = workspace;
            instance.localIntent = intentData;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                // TODO: Can't create intent with spaces in name

                conversation.CreateIntent(HandleSuccessCallback,
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
                // TODO: check if failed because exists, if so update instead
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
