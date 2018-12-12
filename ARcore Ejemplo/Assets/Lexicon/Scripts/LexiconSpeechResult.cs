// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class LexiconSpeechResult
    {
        /// <summary>
        /// Transcription of the utterance.
        /// This will be a partial transcript until isFinal is true.
        /// </summary>
        public string Transcript { get; set; }

        /// <summary>
        /// Array of word results, in transcript order.
        /// </summary>
        public WordResult[] WordResults { get; set; }

        /// <summary>
        /// If true, the results for this utterance will not be updated further.
        /// </summary>
        public bool IsFinal { get; set; }

        /// <summary>
        /// Confidence score for best result, a value between 0 and 1.
        /// Only available once isFinal is true.
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// The start time of this utterance in the context of Time.realtimeSinceStartup.
        /// </summary>
        public float RealtimeStart { get; set; }

        /// <summary>
        /// The end time of this utterance in the context of Time.realtimeSinceStartup.
        /// </summary>
        public float RealtimeEnd { get; set; }

        /// <summary>
        /// An array of detected keyword results.
        /// </summary>
        public KeywordResult[] KeywordResults { get; set; }

        /// <summary>
        /// An array of alternative words results.
        /// </summary>
        public WordAlternativeResults[] AlternativeWordResults { get; set; }

        /// <summary>
        /// Returns all words between the character indices start and stop.
        /// </summary>
        public WordResult[] GetWordsFromTranscriptPositions(int startPosition, int endPosition)
        {
            List<WordResult> results = new List<WordResult>();

            if (startPosition >= 0 && endPosition < Transcript.Length && startPosition < endPosition)
            {
                string substring = Transcript.Substring(0, startPosition).Trim();
                int firstWordIndex = substring.Length > 0 ? substring.Split(' ').Length : 0;
                int wordCount = Transcript.Substring(startPosition, endPosition - startPosition + 1).Trim().Split(' ').Length;

                for (int i = firstWordIndex; i < firstWordIndex + wordCount; i++)
                {
                    if (i >= WordResults.Length)
                    {
                        Debug.LogWarning("transcript positions: " + startPosition + ", " + endPosition + ", " + Transcript.Length);
                        continue;
                    }
                    results.Add(WordResults[i]);
                }
            }

            return results.ToArray();
        }

        public class WordResult
        {
            /// <summary>
            /// The hypothesized word.
            /// </summary>
            public string Word { get; set; }

            /// <summary>
            /// Confidence score for this word, a value between 0 and 1.
            /// </summary>
            public float Confidence { get; set; }

            /// <summary>
            /// Start time for this word in the context of the utterance, in seconds.
            /// </summary>
            public float TimeStart { get; set; }

            /// <summary>
            /// End time for this word in the context of the utterance, in seconds.
            /// </summary>
            public float TimeEnd { get; set; }

            /// <summary>
            /// Start time for this word in the context of Time.realtimeSinceStartup.
            /// </summary>
            public float RealtimeStart { get; set; }

            /// <summary>
            /// End time for this word in the context of Time.realtimeSinceStartup.
            /// </summary>
            public float RealtimeEnd { get; set; }
        }

        public class WordAlternativeResults
        {
            /// <summary>
            /// Start time for this word in the context of the utterance, in seconds.
            /// </summary>
            public float TimeStart { get; set; }

            /// <summary>
            /// End time for this word in the context of the utterance, in seconds.
            /// </summary>
            public float TimeEnd { get; set; }

            /// <summary>
            /// Start time for this word in the context of Time.realtimeSinceStartup.
            /// </summary>
            public float RealtimeStart { get; set; }

            /// <summary>
            /// End time for this word in the context of Time.realtimeSinceStartup.
            /// </summary>
            public float RealtimeEnd { get; set; }

            /// <summary>
            /// Array of possible words for this timeslice.
            /// </summary>
            public WordAlternative[] Alternatives { get; set; }
        }

        public class WordAlternative
        {
            /// <summary>
            /// The hypothesized word.
            /// </summary>
            public string Word { get; set; }

            /// <summary>
            /// Confidence score for this word, a value between 0 and 1.
            /// </summary>
            public float Confidence { get; set; }
        }

        public class KeywordResult
        {
            /// <summary>
            /// The requested keyword.
            /// </summary>
            public string Keyword { get; set; }

            /// <summary>
            /// The keyword as it appears in the transcript.
            /// </summary>
            public string TranscriptText { get; set; }

            /// <summary>
            /// Confidence score for this keyword match, a value between 0 and 1.
            /// </summary>
            public float Confidence { get; set; }

            /// <summary>
            /// Start time for this keyword in the context of the utterance, in seconds.
            /// </summary>
            public float TimeStart { get; set; }

            /// <summary>
            /// End time for this keyword in the context of the utterance, in seconds.
            /// </summary>
            public float TimeEnd { get; set; }

            /// <summary>
            /// Start time for this keyword in the context of Time.realtimeSinceStartup.
            /// </summary>
            public float RealtimeStart { get; set; }

            /// <summary>
            /// End time for this keyword in the context of Time.realtimeSinceStartup.
            /// </summary>
            public float RealtimeEnd { get; set; }
        }
    }
}
