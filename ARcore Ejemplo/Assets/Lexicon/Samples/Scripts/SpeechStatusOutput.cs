// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using Mixspace.Lexicon;
using UnityEngine;
using UnityEngine.UI;

namespace Mixspace.Lexicon.Samples
{
    public class SpeechStatusOutput : MonoBehaviour
    {
        public Text outputText;

        void OnEnable()
        {
            LexiconRuntime.OnSpeechStatusUpdated += OnSpeechStatusUpdated;
        }

        void OnDisable()
        {
            LexiconRuntime.OnSpeechStatusUpdated -= OnSpeechStatusUpdated;
        }

        void OnSpeechStatusUpdated(LexiconSpeechStatus status)
        {
            outputText.text = "Status: " + status.ToString();
        }
    }
}
