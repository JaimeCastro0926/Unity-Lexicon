// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class LexiconRuntimeResult
    {
        /// <summary>
        /// The phrase that was uttered by the user.
        /// </summary>
        public string Utterance { get; set; }

        /// <summary>
        /// The intent that matched this utterance.
        /// </summary>
        public LexiconIntent Intent { get; set; }

        /// <summary>
        /// Confidence score for the intent match, a value between 0 and 1.
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// The entities that were found in this utterance.
        /// </summary>
        /// <remarks>
        /// These belong to the matched intent.
        /// </remarks>
        public List<LexiconEntityMatch> EntityMatches { get; set; }

        /// <summary>
        /// The full speech-to-text result for this utterance.
        /// </summary>
        public LexiconSpeechResult SpeechResult { get; set; }

        public LexiconRuntimeResult()
        {
            EntityMatches = new List<LexiconEntityMatch>();
        }

        /// <summary>
        /// Returns the first entity match for the given entity name (case-insensitive).
        /// </summary>
        public LexiconEntityMatch GetEntityMatch(string entityName)
        {
            if (!string.IsNullOrEmpty(entityName))
            {
                foreach (LexiconEntityMatch entityMatch in EntityMatches)
                {
                    if (string.Equals(entityMatch.Entity.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
                    {
                        return entityMatch;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all the entity matches for the given entity name (case-insensitive).
        /// </summary>
        public List<LexiconEntityMatch> GetEntityMatches(string entityName)
        {
            List<LexiconEntityMatch> matches = new List<LexiconEntityMatch>();

            if (!string.IsNullOrEmpty(entityName))
            {
                foreach (LexiconEntityMatch entityMatch in EntityMatches)
                {
                    if (string.Equals(entityMatch.Entity.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(entityMatch);
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Returns the entity with the given entity name that appears before the target match.
        /// </summary>
        public LexiconEntityMatch GetEntityBefore(string entityName, LexiconEntityMatch targetMatch)
        {
            LexiconEntityMatch match = null;

            if (!string.IsNullOrEmpty(entityName))
            {
                foreach (LexiconEntityMatch entityMatch in EntityMatches)
                {
                    if (entityMatch == targetMatch)
                    {
                        // Break once we hit the target match.
                        break;
                    }

                    if (string.Equals(entityMatch.Entity.EntityName, targetMatch.Entity.EntityName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Clear the match if we run into another entity of the same type.
                        // E.g. for "Create a red cube and a sphere" color should be null before sphere.
                        match = null;
                    }

                    if (string.Equals(entityMatch.Entity.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
                    {
                        match = entityMatch;
                    }
                }
            }

            return match;
        }

        /// <summary>
        /// Returns the entity with the given entity name that appears after the target match.
        /// </summary>
        public LexiconEntityMatch GetEntityAfter(string entityName, LexiconEntityMatch targetMatch)
        {
            LexiconEntityMatch match = null;

            // TODO: I don't like this null check, or the return

            if (!string.IsNullOrEmpty(entityName))
            {
                for (int i = EntityMatches.IndexOf(targetMatch) + 1; i < EntityMatches.Count; i++)
                {
                    LexiconEntityMatch entityMatch = EntityMatches[i];

                    if (string.Equals(entityMatch.Entity.EntityName, targetMatch.Entity.EntityName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Break if we run into another entity of the same type.
                        // E.g. for "Create a cube with a sphere on top" location should be null after cube.
                        break;
                    }

                    if (string.Equals(entityMatch.Entity.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
                    {
                        match = entityMatch;
                        break;
                    }
                }
            }

            return match;
        }

        public List<LexiconEntityMatch> GetEntitiesAfter(string entityName, LexiconEntityMatch targetMatch)
        {
            List<LexiconEntityMatch> matches = new List<LexiconEntityMatch>();

            if (!string.IsNullOrEmpty(entityName))
            {
                for (int i = EntityMatches.IndexOf(targetMatch) + 1; i < EntityMatches.Count; i++)
                {
                    LexiconEntityMatch entityMatch = EntityMatches[i];

                    if (string.Equals(entityMatch.Entity.EntityName, targetMatch.Entity.EntityName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Break if we run into another entity of the same type.
                        // E.g. for "Create a cube with a sphere on top" location should be null after cube.
                        break;
                    }

                    if (string.Equals(entityMatch.Entity.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(entityMatch);
                    }
                }
            }

            return matches;
        }

        // TODO: GetEntitiesBefore
    }
}
