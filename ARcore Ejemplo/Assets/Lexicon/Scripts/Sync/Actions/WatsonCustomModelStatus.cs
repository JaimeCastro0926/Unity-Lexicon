// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCustomModelStatus : WatsonSyncAction
    {
        [NonSerialized]
        private SyncQueue queue;

        public static WatsonCustomModelStatus CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCustomModelStatus instance = CreateInstance<WatsonCustomModelStatus>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            this.queue = queue;

            if (CreateSpeechToText())
            {
                speechToText.GetCustomization(HandleSuccessCallback,
                                              HandleFailCallback,
                                              workspace.WatsonSpeechToTextManager.CustomizationId);
            }
        }

        void HandleSuccessCallback(IBM.Watson.DeveloperCloud.Services.SpeechToText.v1.Customization response, Dictionary<string, object> customData)
        {
            if (response != null)
            {
                string status = response.status;
                if (status.Length > 0)
                {
                    status = status.Substring(0, 1).ToUpper() + status.Substring(1);
                }

                workspace.WatsonSpeechToTextManager.SyncStatus = status;

                Debug.Log("Response progress: " + response.progress);

                // TODO: Use retry here.
                if (response.progress < 100)
                {
                    queue.Insert(TimerSyncAction.CreateInstance(5), 0);
                    queue.Insert(WatsonCustomModelStatus.CreateInstance(workspace), 1);
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
