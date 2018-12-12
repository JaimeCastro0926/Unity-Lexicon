// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonSyncQueue : SyncQueue
    {
        [SerializeField]
        public LexiconWorkspace workspace;

        [SerializeField]
        public WorkspaceSyncData syncData;

        /*
        [NonSerialized]
        private SpeechToText speechToText;

        [NonSerialized]
        private CustomConversation conversation;

        private static string conversationVersionDate = "2017-05-26";

        public SpeechToText SpeechToText()
        {
            if (speechToText != null)
            {
                return speechToText;
            }

            if (workspace.watsonSpeechToTextManager.username == "" || workspace.watsonSpeechToTextManager.password == "")
            {
                Debug.LogError("Watson Speech to Text username and password required to sync");
                return null;
            }

            Credentials credentials = new Credentials(workspace.watsonSpeechToTextManager.username,
                                                      workspace.watsonSpeechToTextManager.password,
                                                      workspace.watsonSpeechToTextManager.url);

            speechToText = new SpeechToText(credentials);

            return speechToText;
        }

        public CustomConversation Conversation()
        {
            if (conversation != null)
            {
                return conversation;
            }

            if (workspace.watsonConversationManager.username == "" || workspace.watsonConversationManager.password == "")
            {
                Debug.LogError("Watson Conversation username and password required to sync");
                return null;
            }

            Credentials credentials = new Credentials(workspace.watsonConversationManager.username,
                                                      workspace.watsonConversationManager.password,
                                                      workspace.watsonConversationManager.url);

            conversation = new CustomConversation(credentials);
            conversation.VersionDate = conversationVersionDate;

            return conversation;
        }
        */
    }
}

#endif
