// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mixspace.Lexicon
{
    [System.Serializable]
    public class LexiconIntentEvent : UnityEvent<LexiconRuntimeResult>
    {
    }

    /// <summary>
    /// Use this component to handle intent matches directly in a scene.
    /// </summary>
    public class LexiconIntentHandler : MonoBehaviour
    {
        public LexiconIntent intent;

        public LexiconIntentEvent process;

        void OnEnable()
        {
            LexiconRuntime.OnLexiconResults += OnLexiconResults;
        }

        void OnDisable()
        {
            LexiconRuntime.OnLexiconResults -= OnLexiconResults;
        }

        void OnLexiconResults(List<LexiconRuntimeResult> results)
        {
            foreach (LexiconRuntimeResult result in results)
            {
                if (result.Intent == intent)
                {
                    process.Invoke(result);
                }
            }
        }
    }
}
