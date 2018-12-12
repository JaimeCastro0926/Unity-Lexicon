// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class LexiconEntityMatch
    {
        /// <summary>
        /// The entity that was matched.
        /// </summary>
        public LexiconEntity Entity { get; set; }

        /// <summary>
        /// The value of the entity.
        /// </summary>
        public LexiconEntityValue EntityValue { get; set; }

        /// <summary>
        /// If entity is a system entity (e.g. sys-number) this will return the parsed value.
        /// </summary>
        public SystemEntityValue SystemValue
        {
            get
            {
                if (EntityValue is SystemEntityValue)
                {
                    return (SystemEntityValue)EntityValue;
                }
                return null;
            }
        }

        /// <summary>
        /// The position of the value's first character in the transcription.
        /// </summary>
        public int FirstCharacter { get; set; }

        /// <summary>
        /// The position of the value's last character in the transcription.
        /// </summary>
        public int LastCharacter { get; set; }

        /// <summary>
        /// The start time (in seconds) measured from the start of the utterance.
        /// </summary>
        public float TimeStart { get; set; }

        /// <summary>
        /// The end time (in seconds) measured from the start of the utterance.
        /// </summary>
        public float TimeEnd { get; set; }

        /// <summary>
        /// The start time (in seconds) measured from application start time (Time.realtimeSinceStartup).
        /// </summary>
        public float RealtimeStart { get; set; }

        /// <summary>
        /// The end time (in seconds) measured from application start time (Time.realtimeSinceStartup).
        /// </summary>
        public float RealtimeEnd { get; set; }

        /// <summary>
        /// The focus position for the moment this entity was uttered (may be null).
        /// </summary>
        public FocusPosition FocusPosition { get; set; }

        /// <summary>
        /// The focus selection for the moment this entity was uttered (may be null).
        /// </summary>
        public FocusSelection FocusSelection { get; set; }

        public override string ToString()
        {
            return Entity.EntityName + ": " + EntityValue.ValueName + " " + TimeStart + " (" + RealtimeStart + ")";
        }
    }
}
