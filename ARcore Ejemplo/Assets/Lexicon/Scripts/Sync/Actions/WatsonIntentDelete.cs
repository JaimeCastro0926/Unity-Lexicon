// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonIntentDelete : WatsonSyncAction
    {
        public string intentName;

        public static WatsonIntentDelete CreateInstance(LexiconWorkspace workspace, string intentName)
        {
            WatsonIntentDelete instance = CreateInstance<WatsonIntentDelete>();
            instance.workspace = workspace;
            instance.intentName = intentName;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                UnityEngine.Debug.Log("Deleting Intent: " + intentName);
                conversation.DeleteIntent(HandleSuccessCallback,
                                          HandleFailCallback,
                                          workspace.WatsonConversationManager.WorkspaceId,
                                          intentName);
            }
        }

        void HandleSuccessCallback(bool success, Dictionary<string, object> customData)
        {
            if (success)
            {
                // TODO: figure this out...
                //workspace.watsonConversationManager.RemoveIntentTimestamp(intentName);
            }

            succeeded = success;
            isDone = true;
        }

        void HandleFailCallback(RESTConnector.Error error, Dictionary<string, object> customData)
        {
            if (error == null)
            {
                // Probably succeeded, but getting wrong code from server.
                // TODO: Follow up with Watson SDK.
                succeeded = true;
            }
            else
            {
                Debug.LogError(error);
                succeeded = false;
            }

            isDone = true;
        }
    }
}

#endif
