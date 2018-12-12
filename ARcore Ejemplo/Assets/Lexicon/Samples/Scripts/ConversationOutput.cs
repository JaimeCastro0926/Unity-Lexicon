// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections.Generic;
using System.Text;
using Mixspace.Lexicon;
using UnityEngine;
using UnityEngine.UI;

namespace Mixspace.Lexicon.Samples
{
    public class ConversationOutput : MonoBehaviour
    {
        public LexiconEntity selectionEntity;
        public LexiconEntity locationEntity;

        public Text outputText;

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
            if (results.Count == 0)
            {
                return;
            }

            LexiconRuntimeResult result = results[0];

            StringBuilder builder = new StringBuilder();

            builder.AppendLine(string.Format("Intent: {0} ({1:F2})", result.Intent.IntentName, result.Confidence));

            foreach (LexiconEntityMatch entityMatch in result.EntityMatches)
            {
                if (entityMatch.Entity == selectionEntity)
                {
                    if (entityMatch.FocusSelection == null)
                    {
                        builder.AppendLine("  " + entityMatch.Entity.EntityName + ": null");
                    }
                    else
                    {
                        builder.AppendLine("  " + entityMatch.Entity.EntityName + ": " + entityMatch.FocusSelection.SelectedObject.name);
                    }
                }
                else if (entityMatch.Entity == locationEntity)
                {
                    if (entityMatch.FocusPosition == null)
                    {
                        builder.AppendLine("  " + entityMatch.Entity.EntityName + ": null");
                    }
                    else
                    {
                        builder.AppendLine("  " + entityMatch.Entity.EntityName + ": " + entityMatch.FocusPosition.Position);
                    }
                }
                else
                {
                    builder.AppendLine("  " + entityMatch.Entity.EntityName + ": " + entityMatch.EntityValue.ValueName);
                }
            }

            outputText.text = builder.ToString();
        }
    }
}
