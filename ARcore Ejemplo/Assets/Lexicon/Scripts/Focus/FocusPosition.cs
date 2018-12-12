// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    /// <summary>
    /// Focus data with position.
    /// </summary>
    public class FocusPosition : LexiconFocusData
    {
        /// <summary>
        /// The focus position, typically in world space.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// The surface normal at this position.
        /// </summary>
        public Vector3 Normal { get; set; }
    }
}
