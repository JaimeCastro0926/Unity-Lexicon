// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon.Samples
{
    // Sample script for indicating selection from partial speech results.
    // Useful for giving the user early feedback.
    public class SelectionDetector : MonoBehaviour
    {
        public LexiconEntity selectionEntity;

        private LexiconFocusManager focusManager;

        private List<LexiconSelectable> selectedObjects = new List<LexiconSelectable>();

        void OnEnable()
        {
            LexiconRuntime.OnSpeechToTextResults += OnSpeechToTextResult;
            LexiconRuntime.OnLexiconResults += OnLexiconResults;
        }

        void OnDisable()
        {
            LexiconRuntime.OnSpeechToTextResults -= OnSpeechToTextResult;
            LexiconRuntime.OnLexiconResults -= OnLexiconResults;
        }

        void Start()
        {
            if (selectionEntity == null)
            {
                Debug.LogError("SelectionDetector requires a Selection entity");
                enabled = false;
            }

            focusManager = LexiconFocusManager.Instance;
        }

        void OnSpeechToTextResult(LexiconSpeechResult speechResult)
        {
            foreach (LexiconSpeechResult.WordResult wordResult in speechResult.WordResults)
            {
                if (selectionEntity.FindValueByName(wordResult.Word, true) != null)
                {
                    FocusSelection focusSelection = focusManager.GetFocusData<FocusSelection>(wordResult.RealtimeStart);
                    if (focusSelection != null)
                    {
                        LexiconSelectable selectable = focusSelection.SelectedObject.GetComponent<LexiconSelectable>();
                        selectable.Select();
                        selectedObjects.Add(selectable);
                    }
                }
            }
        }

        void OnLexiconResults(List<LexiconRuntimeResult> results)
        {
            foreach (LexiconSelectable selectable in selectedObjects)
            {
                selectable.Deselect();
            }
            selectedObjects.Clear();
        }
    }
}