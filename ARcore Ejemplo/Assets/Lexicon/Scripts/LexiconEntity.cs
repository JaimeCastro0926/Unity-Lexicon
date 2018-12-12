// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    public abstract class LexiconEntity : ScriptableObject
    {
        [SerializeField]
        private string entityName;

        /// <summary>
        /// The name of the entity (e.g. Color).
        /// </summary>
        public string EntityName
        {
            get { return entityName; }
        }

        /// <summary>
        /// The values of the entity.
        /// </summary>
        public abstract LexiconEntityValue[] Values
        {
            get;
        }

        /// <summary>
        /// Returns the value that matches name (case-insensitive).
        /// </summary>
        public virtual LexiconEntityValue FindValueByName(string name, bool searchSynonyms = false)
        {
            if (Values != null)
            {
                foreach (LexiconEntityValue value in Values)
                {
                    if (name.Equals(value.ValueName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return value;
                    }

                    if (searchSynonyms)
                    {
                        foreach (string synonym in value.GetSynonymList())
                        {
                            if (name.Equals(synonym, System.StringComparison.OrdinalIgnoreCase))
                            {
                                return value;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
