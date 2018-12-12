// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    public class LexiconConfig : ScriptableObject
    {
        [SerializeField]
        private string watsonSpeechToTextUsername;

        [SerializeField]
        private string watsonSpeechToTextPassword;

        [SerializeField]
        private string watsonConversationUsername;

        [SerializeField]
        private string watsonConversationPassword;

        public string WatsonSpeechToTextUsername
        {
            get { return watsonSpeechToTextUsername; }
            set { watsonSpeechToTextUsername = value; }
        }

        public string WatsonSpeechToTextPassword
        {
            get { return watsonSpeechToTextPassword; }
            set { watsonSpeechToTextPassword = value; }
        }

        public string WatsonConversationUsername
        {
            get { return watsonConversationUsername; }
            set { watsonConversationUsername = value; }
        }

        public string WatsonConversationPassword
        {
            get { return watsonConversationPassword; }
            set { watsonConversationPassword = value; }
        }
    }
}
