// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

// Shorthand for the auto-generated strings belonging to this intent.
using Strings = Mixspace.Lexicon.Actions.ResizeStrings;

namespace Mixspace.Lexicon.Actions
{
    public class ResizeAction : LexiconAction
    {
        public override bool Process(LexiconRuntimeResult runtimeResult)
        {
            // There may be multiple size matches, break the phrase up by size entity.
            // E.g. "Make this one bigger and this one smaller."
            foreach (LexiconEntityMatch sizeMatch in runtimeResult.GetEntityMatches(Strings.Size))
            {
                // Default focus on the size entity.
                FocusSelection focusSelection = sizeMatch.FocusSelection;

                // We expect the selection entity (if present) to come before the size entity.
                LexiconEntityMatch selectionMatch = runtimeResult.GetEntityBefore(Strings.Selection, sizeMatch);
                if (selectionMatch != null)
                {
                    focusSelection = selectionMatch.FocusSelection;
                }

                if (focusSelection != null)
                {
                    GameObject selectedObject = focusSelection.SelectedObject;
                    float scale = 1.0f;

                    switch (sizeMatch.EntityValue.ValueName)
                    {
                        case Strings.SizeValues.Grande:
                            scale = 2.0f;
                            break;
                        case Strings.SizeValues.Pequeño:
                            scale = 0.5f;
                            break;
                    }

                    // Scale the selected object by the desired amount.
                    selectedObject.transform.localScale = selectedObject.transform.localScale * scale;
                }
            }
            
            // We've consumed this intent, return true to prevent other handlers from firing.
            return true;
        }
    }
}
