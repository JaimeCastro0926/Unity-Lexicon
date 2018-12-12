// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonCustomWordsUpdate : WatsonSyncAction
    {
        public static WatsonCustomWordsUpdate CreateInstance(LexiconWorkspace workspace)
        {
            WatsonCustomWordsUpdate instance = CreateInstance<WatsonCustomWordsUpdate>();
            instance.workspace = workspace;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            Words customWords = GetCustomWords(((WatsonSyncQueue)queue).syncData);

            if (customWords.words.Length == 0)
            {
                succeeded = true;
                isDone = true;
                return;
            }

            if (CreateSpeechToText())
            {
                speechToText.AddCustomWords(HandleSuccessCallback,
                                            HandleFailCallback,
                                            workspace.WatsonSpeechToTextManager.CustomizationId,
                                            customWords);
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

        private Words GetCustomWords(WorkspaceSyncData syncData)
        {
            Words words = new Words();
            List<Word> wordList = new List<Word>();

            foreach (LexiconCustomWord customWord in syncData.customWords)
            {
                Word w0 = new Word();
                w0.word = customWord.Word;
                w0.display_as = customWord.DisplayAs;
                w0.sounds_like = customWord.GetPronunciationList().ToArray();
                wordList.Add(w0);
            }

            words.words = wordList.ToArray();

            return words;
        }
    }
}

#endif
