// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public static class LexiconConstants
    {
        public const string Version = "0.5.4";

        public static class PlayerPrefs
        {
            public const string WatsonSpeechToTextUsername = "Mixspace.Lexicon.Watson.SpeechToText.Username";
            public const string WatsonSpeechToTextPassword = "Mixspace.Lexicon.Watson.SpeechToText.Password";

            public const string WatsonConversationUsername = "Mixspace.Lexicon.Watson.Conversation.Username";
            public const string WatsonConversationPassword = "Mixspace.Lexicon.Watson.Conversation.Password";

            public const string ActionNamespace = "Mixspace.Lexicon.ActionNamespace";
            public const string AutoGenerateStrings = "Mixspace.Lexicon.AutoGenerateStrings";
        }

        public static class Files
        {
            public const string TempFolder = "Mixspace_Lexicon_Temp";
            public const string ConfigFile = "mixspace_lexicon_config";
        }
    }
}
