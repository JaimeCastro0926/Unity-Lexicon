// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using Mixspace.Lexicon;
using UnityEngine;
using UnityEngine.UI;

namespace Mixspace.Lexicon.Samples
{
    public class SpeechToTextOutput : MonoBehaviour
    {
        public Text outputText;

        void OnEnable()
        {
            LexiconRuntime.OnSpeechToTextResults += OnSpeechToTextResults;
            LexiconRuntime.OnKeywordDetected += OnKeywordDetected;
        }

        void OnDisable()
        {
            LexiconRuntime.OnSpeechToTextResults -= OnSpeechToTextResults;
            LexiconRuntime.OnKeywordDetected -= OnKeywordDetected;
        }

        void OnSpeechToTextResults(LexiconSpeechResult speechResult)
        {
            outputText.text = speechResult.Transcript;
        }

        void OnKeywordDetected(LexiconSpeechResult.KeywordResult keywordResult)
        {
            outputText.text = "Found Keyword: " + keywordResult.Keyword;
        }
    }
}
