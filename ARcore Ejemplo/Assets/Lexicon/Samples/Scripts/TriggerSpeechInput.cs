// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon.Samples
{
    public class TriggerSpeechInput : MonoBehaviour
    {
        public KeyCode pushToTalkKey;
        public KeyCode singlePhraseCaptureKey;
        public float captureTimeout = 3.0f;
        public bool captureTrigger;
        public bool touchToTalk;

        void Start()
        {
            if (LexiconRuntime.CurrentRuntime)
            {
                LexiconRuntime.CurrentRuntime.SpeechToTextActive = false;
            }
        }

        void Update()
        {
            LexiconRuntime lexiconRuntime = LexiconRuntime.CurrentRuntime;
            if (lexiconRuntime == null)
            {
                return;
            }

            if (Input.GetKeyDown(pushToTalkKey))
            {
                lexiconRuntime.StartSpeechService();
            }

            if (Input.GetKeyUp(pushToTalkKey))
            {
                lexiconRuntime.StopSpeechService(true);
            }

            if (Input.GetKeyDown(singlePhraseCaptureKey))
            {
                lexiconRuntime.CaptureOnePhrase(captureTimeout);
            }

            if (captureTrigger)
            {
                lexiconRuntime.CaptureOnePhrase(captureTimeout);
                captureTrigger = false;
            }

            if (touchToTalk)
            {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    lexiconRuntime.StartSpeechService();
                }

                if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled))
                {
                    lexiconRuntime.StopSpeechService(true);
                }
            }
        }
    }
}
