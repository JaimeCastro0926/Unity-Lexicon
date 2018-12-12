// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using Mixspace.Lexicon.Watson.Conversation.v1;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonSyncAction : SyncAction
    {
        public LexiconWorkspace workspace;

        protected SpeechToText speechToText;

        protected LexiconConversation conversation;
        private static string conversationVersionDate = "2017-05-26";

        public bool CreateSpeechToText()
        {
            if (workspace.WatsonSpeechToTextManager.Username == "" || workspace.WatsonSpeechToTextManager.Password == "")
            {
                Debug.LogError("Watson Speech to Text username and password required to sync");
                succeeded = false;
                isDone = true;
                return false;
            }

            Credentials credentials = new Credentials(workspace.WatsonSpeechToTextManager.Username,
                                                      workspace.WatsonSpeechToTextManager.Password,
                                                      workspace.WatsonSpeechToTextManager.Url);

            speechToText = new SpeechToText(credentials);

            return true;
        }

        public bool CreateConversation()
        {
            if (workspace.WatsonConversationManager.Username == "" || workspace.WatsonConversationManager.Password == "")
            {
                Debug.LogError("Watson Conversation username and password required to sync");
                succeeded = false;
                isDone = true;
                return false;
            }

            Credentials credentials = new Credentials(workspace.WatsonConversationManager.Username,
                                                      workspace.WatsonConversationManager.Password,
                                                      workspace.WatsonConversationManager.Url);

            conversation = new LexiconConversation(credentials);
            conversation.VersionDate = conversationVersionDate;

            return true;
        }

        public override void Process(SyncQueue queue)
        {

        }
    }
}

#endif
