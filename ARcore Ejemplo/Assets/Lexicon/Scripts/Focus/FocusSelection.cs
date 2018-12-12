// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    /// <summary>
    /// Focus data with selected object.
    /// </summary>
    public class FocusSelection : LexiconFocusData
    {
        /// <summary>
        /// The selected object.
        /// </summary>
        public GameObject SelectedObject { get; set; }
    }
}
