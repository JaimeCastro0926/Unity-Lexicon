// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCustomWordsGet : WatsonSyncAction
    {
        public static WatsonCustomWordsGet CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCustomWordsGet instance = CreateInstance<WatsonCustomWordsGet>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (CreateSpeechToText())
            {
                speechToText.GetCustomWords(HandleSuccessCallback,
                                            HandleFailCallback,
                                            workspace.WatsonSpeechToTextManager.CustomizationId);
            }
        }

        void HandleSuccessCallback(WordsList response, System.Collections.Generic.Dictionary<string, object> customData)
        {
            if (response != null)
            {
                workspace.CustomWords.Clear();
                foreach (WordData wordData in response.words)
                {
                    LexiconCustomWord customWord = new LexiconCustomWord();
                    customWord.Word = wordData.word;
                    customWord.DisplayAs = wordData.display_as;
                    customWord.Pronunciations = string.Join(", ", wordData.sounds_like);

                    workspace.CustomWords.Add(customWord);
                }

                EditorUtility.SetDirty(workspace);
                AssetDatabase.SaveAssets();
            }

            succeeded = true;
            isDone = true;
        }

        void HandleFailCallback(RESTConnector.Error error, System.Collections.Generic.Dictionary<string, object> customData)
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
