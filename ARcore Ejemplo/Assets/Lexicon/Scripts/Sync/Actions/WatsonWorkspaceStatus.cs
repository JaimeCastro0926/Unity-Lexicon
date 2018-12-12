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
    public class WatsonWorkspaceStatus : WatsonSyncAction
    {
        [NonSerialized]
        private SyncQueue queue;

        public static WatsonWorkspaceStatus CreateInstance(LexiconWorkspace workspace)
        {
            WatsonWorkspaceStatus instance = CreateInstance<WatsonWorkspaceStatus>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            this.queue = queue;

            if (CreateConversation())
            {
                conversation.GetWorkspace(HandleSuccessCallback,
                                          HandleFailCallback,
                                          workspace.WatsonConversationManager.WorkspaceId,
                                          false);
            }
        }

        void HandleSuccessCallback(Workspace watsonWorkspace, Dictionary<string, object> customData)
        {
            if (watsonWorkspace != null)
            {
                workspace.WatsonConversationManager.SyncStatus = watsonWorkspace.status;

                if (!string.Equals(watsonWorkspace.status, "Available", StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: Use retry here.
                    queue.Insert(TimerSyncAction.CreateInstance(10), 0);
                    queue.Insert(WatsonWorkspaceStatus.CreateInstance(workspace), 1);
                }

                succeeded = true;
                isDone = true;
                return;
            }

            succeeded = false;
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
