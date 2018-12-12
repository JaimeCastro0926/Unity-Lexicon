// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    /// <summary>
    /// Focus data entry for the FocusManager. Create a subclass to add additional data.
    /// </summary>
    public class LexiconFocusData
    {
        /// <summary>
        /// Timestamp in terms of Time.realtimeSinceStartup.
        /// </summary>
        public float Timestamp { get; set; }

        /// <summary>
        /// Is this from the data pool?
        /// </summary>
        public bool IsPooled { get; set; }

        /// <summary>
        /// Creating a new FocusData entry automatically sets the timestamp.
        /// </summary>
        public LexiconFocusData()
        {
            Timestamp = Time.realtimeSinceStartup;
        }
    }
}
