// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

// Shorthand for the auto-generated strings belonging to this intent.
using Strings = Mixspace.Lexicon.Actions.MoveStrings;

namespace Mixspace.Lexicon.Actions
{
    public class MoveAction : LexiconAction
    {
        public override bool Process(LexiconRuntimeResult runtimeResult)
        {
            // There may be multiple selections, break the phrase up by selection entity.
            // E.g. "Move this one over here and this one over here."
            foreach (LexiconEntityMatch selectionMatch in runtimeResult.GetEntityMatches(Strings.Selection))
            {
                // We expect the position entity to come after the selection entity.
                LexiconEntityMatch positionMatch = runtimeResult.GetEntityAfter(Strings.Position, selectionMatch);
                if (positionMatch != null)
                {
                    FocusSelection focusSelection = selectionMatch.FocusSelection;
                    FocusPosition focusPosition = positionMatch.FocusPosition;

                    if (focusSelection != null && focusPosition != null)
                    {
                        float floorAngle = Vector3.Angle(Vector3.up, focusPosition.Normal);
                        if (floorAngle < 45)
                        {
                            focusSelection.SelectedObject.transform.position = focusPosition.Position;
                        }
                        else
                        {
                            // Match the normal with vertical surfaces.
                            focusSelection.SelectedObject.transform.position = focusPosition.Position;
                            focusSelection.SelectedObject.transform.forward = -focusPosition.Normal;
                        }
                    }
                }
            }

            // We've consumed this intent, return true to prevent other handlers from firing.
            return true;
        }
    }
}
