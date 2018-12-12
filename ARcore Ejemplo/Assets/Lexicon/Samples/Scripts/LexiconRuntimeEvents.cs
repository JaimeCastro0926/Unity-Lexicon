// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections.Generic;
using Mixspace.Lexicon;
using UnityEngine;

namespace Mixspace.Lexicon.Samples
{
    public class LexiconRuntimeEvents : MonoBehaviour
    {
        void OnEnable()
        {
            LexiconRuntime.OnSpeechStatusUpdated += OnSpeechStatusUpdated;
            LexiconRuntime.OnSpeechToTextResults += OnSpeechToTextResults;
            LexiconRuntime.OnKeywordDetected += OnKeywordDetected;
            LexiconRuntime.OnLexiconResults += OnLexiconResults;
        }

        void OnDisable()
        {
            LexiconRuntime.OnSpeechStatusUpdated -= OnSpeechStatusUpdated;
            LexiconRuntime.OnSpeechToTextResults -= OnSpeechToTextResults;
            LexiconRuntime.OnKeywordDetected -= OnKeywordDetected;
            LexiconRuntime.OnLexiconResults -= OnLexiconResults;
        }

        void OnSpeechStatusUpdated(LexiconSpeechStatus status)
        {
            Debug.Log("Speech Status: " + status);
        }

        void OnSpeechToTextResults(LexiconSpeechResult speechResult)
        {
            if (speechResult.IsFinal)
            {
                Debug.Log("Speech Transcript: " + speechResult.Transcript);
            }
        }

        void OnKeywordDetected(LexiconSpeechResult.KeywordResult keywordResult)
        {
            Debug.Log("Found Keyword: " + keywordResult.Keyword);
        }

        void OnLexiconResults(List<LexiconRuntimeResult> results)
        {
            foreach (LexiconRuntimeResult result in results)
            {
                Debug.Log("Matched Intent: " + result.Intent.ActionName + ", confidence: " + result.Confidence);
            }
        }
    }
}
