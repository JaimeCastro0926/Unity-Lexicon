// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCustomModelCreate : WatsonSyncAction
    {
        public static WatsonCustomModelCreate CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCustomModelCreate instance = CreateInstance<WatsonCustomModelCreate>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (!string.IsNullOrEmpty(workspace.WatsonSpeechToTextManager.CustomizationId))
            {
                Debug.LogError("Watson custom model already exists");
                succeeded = false;
                isDone = true;
                return;
            }

            if (CreateSpeechToText())
            {
                speechToText.CreateCustomization(HandleSuccessCallback,
                                                 HandleFailCallback,
                                                 workspace.WorkspaceName,
                                                 workspace.WatsonSpeechToTextManager.Model);
            }
        }

        void HandleSuccessCallback(IBM.Watson.DeveloperCloud.Services.SpeechToText.v1.CustomizationID response, Dictionary<string, object> customData)
        {
            if (response != null)
            {
                workspace.WatsonSpeechToTextManager.CustomizationId = response.customization_id;
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
