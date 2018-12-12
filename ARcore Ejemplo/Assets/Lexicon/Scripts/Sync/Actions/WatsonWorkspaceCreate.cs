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
    public class WatsonWorkspaceCreate : WatsonSyncAction
    {
        public static WatsonWorkspaceCreate CreateInstance(LexiconWorkspace workspace)
        {
            WatsonWorkspaceCreate instance = CreateInstance<WatsonWorkspaceCreate>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (!string.IsNullOrEmpty(workspace.WatsonConversationManager.WorkspaceId))
            {
                Debug.LogError("Watson workspace already exists");
                succeeded = false;
                isDone = true;
                return;
            }

            if (CreateConversation())
            {
                conversation.CreateWorkspace(HandleSuccessCallback,
                                             HandleFailCallback,
                                             workspace.WorkspaceName,
                                             workspace.WatsonConversationManager.Language,
                                             workspace.WorkspaceDescription);
            }
        }

        void HandleSuccessCallback(Workspace watsonWorkspace, Dictionary<string, object> customData)
        {
            if (watsonWorkspace != null)
            {
                workspace.WatsonConversationManager.WorkspaceId = watsonWorkspace.workspace_id;
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
