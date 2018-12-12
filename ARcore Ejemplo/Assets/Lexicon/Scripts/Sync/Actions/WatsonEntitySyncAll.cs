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
    public class WatsonEntitySyncAll : WatsonSyncAction
    {
        [NonSerialized]
        private WatsonSyncQueue queue;

        public static WatsonEntitySyncAll CreateInstance(LexiconWorkspace workspace)
        {
            WatsonEntitySyncAll instance = CreateInstance<WatsonEntitySyncAll>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                this.queue = (WatsonSyncQueue)queue;

                conversation.ListEntities(HandleSuccessCallback,
                                          HandleFailCallback,
                                          workspace.WatsonConversationManager.WorkspaceId,
                                          false);
            }
        }

        void HandleSuccessCallback(ListEntitiesResp entitiesResp, Dictionary<string, object> customData)
        {
            if (entitiesResp == null)
            {
                succeeded = false;
                isDone = true;
                return;
            }

            WorkspaceSyncData syncData = queue.syncData;

            HashSet<string> watsonEntityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Entity watsonEntity in entitiesResp.entities)
            {
                string entityName = watsonEntity.entity;
                EntityData localEntity;

                if (syncData.entityData.TryGetValue(entityName, out localEntity))
                {
                    string localTimestamp = workspace.WatsonConversationManager.Timestamps.GetEntityTimestamp(entityName);
                    bool needsUpdate = true;

                    if (watsonEntity.updated.Equals(localTimestamp, StringComparison.OrdinalIgnoreCase))
                    {
                        // No remote changes

                        if (workspace.WatsonConversationManager.LastSyncData != null)
                        {
                            EntityData lastSync;
                            if (workspace.WatsonConversationManager.LastSyncData.entityData.TryGetValue(entityName, out lastSync))
                            {
                                if (WorkspaceSyncData.CompareEntityData(localEntity, lastSync))
                                {
                                    // No local changes
                                    needsUpdate = false;
                                }
                            }
                        }
                    }

                    if (needsUpdate)
                    {
                        queue.Enqueue(WatsonEntityUpdate.CreateInstance(workspace, localEntity));
                    }
                }
                else
                {
                    // Entity deleted locally
                    queue.Enqueue(WatsonEntityDelete.CreateInstance(workspace, entityName));
                }

                watsonEntityNames.Add(entityName);
            }

            foreach (EntityData localEntity in syncData.entityData.Values)
            {
                if (!watsonEntityNames.Contains(localEntity.name))
                {
                    // Entity does not exist on server
                    queue.Enqueue(WatsonEntityCreate.CreateInstance(workspace, localEntity));
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
