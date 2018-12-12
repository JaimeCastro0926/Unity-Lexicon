// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCorpusAddEntities : WatsonSyncAction
    {
        public static WatsonCorpusAddEntities CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCorpusAddEntities instance = CreateInstance<WatsonCorpusAddEntities>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateSpeechToText())
            {
                string corpusName = workspace.CanonicalName + "_Entities";
                string corpusData = CreateCorpusData(((WatsonSyncQueue)queue).syncData);

                speechToText.AddCustomCorpus(HandleSuccessCallback,
                                             HandleFailCallback,
                                             workspace.WatsonSpeechToTextManager.CustomizationId,
                                             corpusName,
                                             true,
                                             corpusData);
            }
        }

        void HandleSuccessCallback(bool response, Dictionary<string, object> customData)
        {
            succeeded = response;
            isDone = true;
        }

        void HandleFailCallback(RESTConnector.Error error, Dictionary<string, object> customData)
        {
            Debug.Log(error);

            if (error.ErrorCode == 409)
            {
                Debug.Log("Retrying in 5 seconds");
                retry = true;
                retryDelay = 5.0f;
            }

            succeeded = false;
            isDone = true;
        }

        private string CreateCorpusData(WorkspaceSyncData syncData)
        {
            StringBuilder builder = new StringBuilder();

            foreach (EntityData entity in syncData.entityData.Values)
            {
                foreach (EntityValueData entityValue in entity.values)
                {
                    builder.AppendLine(entityValue.name);

                    foreach (string synonym in entityValue.synonyms)
                    {
                        builder.AppendLine(synonym);
                    }
                }
            }

            return builder.ToString();
        }
    }
}

#endif
