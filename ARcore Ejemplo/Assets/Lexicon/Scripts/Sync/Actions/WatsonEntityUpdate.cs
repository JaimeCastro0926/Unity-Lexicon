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
    public class WatsonEntityUpdate : WatsonSyncAction
    {
        public EntityData localEntity;

        public static WatsonEntityUpdate CreateInstance(LexiconWorkspace workspace, EntityData entityData)
        {
            WatsonEntityUpdate instance = CreateInstance<WatsonEntityUpdate>();
            instance.workspace = workspace;
            instance.localEntity = entityData;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                // TODO: Would be better to update values individually instead of replacing all

                List<CreateValue> createValues = new List<CreateValue>();

                foreach (EntityValueData valueData in localEntity.values)
                {
                    CreateValue createValue = new CreateValue();
                    createValue.value = valueData.name;
                    createValue.synonyms = valueData.synonyms.ToArray();
                    createValues.Add(createValue);
                }

                conversation.UpdateEntity(HandleSuccessCallback,
                                          HandleFailCallback,
                                          workspace.WatsonConversationManager.WorkspaceId,
                                          localEntity.name,
                                          null,
                                          createValues.ToArray());
            }
        }

        void HandleSuccessCallback(Entity watsonEntity, Dictionary<string, object> customData)
        {
            if (watsonEntity != null)
            {
                workspace.WatsonConversationManager.Timestamps.SetEntityTimestamp(localEntity.name, watsonEntity.updated);
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
