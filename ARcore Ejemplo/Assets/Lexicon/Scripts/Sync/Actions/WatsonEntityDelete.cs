// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonEntityDelete : WatsonSyncAction
    {
        public string entityName;

        public static WatsonEntityDelete CreateInstance(LexiconWorkspace workspace, string entityName)
        {
            WatsonEntityDelete instance = CreateInstance<WatsonEntityDelete>();
            instance.workspace = workspace;
            instance.entityName = entityName;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                UnityEngine.Debug.Log("Deleting Entity: " + entityName);
                conversation.DeleteEntity(HandleSuccessCallback,
                                          HandleFailCallback,
                                          workspace.WatsonConversationManager.WorkspaceId,
                                          entityName);
            }
        }

        void HandleSuccessCallback(bool success, Dictionary<string, object> customData)
        {
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
