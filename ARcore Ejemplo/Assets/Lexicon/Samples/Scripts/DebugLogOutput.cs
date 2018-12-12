// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Mixspace.Lexicon.Samples
{
    public class DebugLogOutput : MonoBehaviour
    {
        public Text outputText;
        public int maxEntries = 5;

        private List<string> logStrings = new List<string>();

        void OnEnable()
        {
            Application.logMessageReceived += LogMessageReceived;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= LogMessageReceived;
        }

        void LogMessageReceived(string logString, string stackTrace, LogType type)
        {
            logStrings.Add(logString);
            if (logStrings.Count >= maxEntries)
            {
                logStrings.RemoveRange(0, logStrings.Count - maxEntries);
            }

            StringBuilder builder = new StringBuilder();

            foreach (string s in logStrings)
            {
                builder.AppendLine();
                builder.Append(s);
            }

            outputText.text = builder.ToString();
        }
    }
}
