// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCustomModelTrain : WatsonSyncAction
    {
        public static WatsonCustomModelTrain CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCustomModelTrain instance = CreateInstance<WatsonCustomModelTrain>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateSpeechToText())
            {
                speechToText.TrainCustomization(HandleSuccessCallback,
                                                HandleFailCallback,
                                                workspace.WatsonSpeechToTextManager.CustomizationId);
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
    }
}

#endif
