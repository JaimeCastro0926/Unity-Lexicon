// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    public class LexiconTextInput : MonoBehaviour
    {
        public string input;

        public void ProcessInput()
        {
            if (LexiconRuntime.CurrentRuntime)
            {
                LexiconRuntime.CurrentRuntime.ProcessInput(input);
            }
        }
    }
}
