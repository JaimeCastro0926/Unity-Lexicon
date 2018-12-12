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
    public class WatsonEntityCreate : WatsonSyncAction
    {
        public EntityData localEntity;

        public static WatsonEntityCreate CreateInstance(LexiconWorkspace workspace, EntityData entityData)
        {
            WatsonEntityCreate instance = CreateInstance<WatsonEntityCreate>();
            instance.workspace = workspace;
            instance.localEntity = entityData;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateConversation())
            {
                List<CreateValue> createValues = new List<CreateValue>();

                foreach (EntityValueData valueData in localEntity.values)
                {
                    CreateValue createValue = new CreateValue();
                    createValue.value = valueData.name;
                    createValue.synonyms = valueData.synonyms.ToArray();
                    createValues.Add(createValue);
                }

                conversation.CreateEntity(HandleSuccessCallback,
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
