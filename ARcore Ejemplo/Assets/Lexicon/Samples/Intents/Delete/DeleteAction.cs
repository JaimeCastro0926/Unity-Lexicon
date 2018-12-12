// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using Strings = Mixspace.Lexicon.Actions.DeleteStrings;

namespace Mixspace.Lexicon.Actions
{
    public class DeleteAction : LexiconAction
    {
        public override bool Process(LexiconRuntimeResult runtimeResult)
        {
            // We want to be really certain before deleting things.
            if (runtimeResult.Confidence < 0.9f)
            {
                return false;
            }

            foreach (LexiconEntityMatch selectionMatch in runtimeResult.GetEntityMatches(Strings.Selection))
            {
                if (selectionMatch.FocusSelection != null)
                {
                    Destroy(selectionMatch.FocusSelection.SelectedObject);
                }
            }
            
            return true;
        }
    }
}
