// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class LexiconEntityValue
    {
        [SerializeField]
        private string valueName;

        [SerializeField]
        private string synonyms;

        /// <summary>
        /// The name of the value (e.g. red).
        /// </summary>
        public string ValueName
        {
            get { return valueName; }
            protected set { valueName = value; }
        }

        /// <summary>
        /// A comma-delimited list of synonyms (e.g. "ruby, crimson").
        /// </summary>
        public string Synonyms
        {
            get { return synonyms; }
        }

        /// <summary>
        /// The value binding.
        /// </summary>
        public virtual object Binding
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the value binding cast to the given type.
        /// </summary>
        public virtual T GetBinding<T>()
        {
            if (Binding.GetType() == typeof(T))
            {
                return (T)Binding;
            }

            Debug.LogWarning("EntityValue " + valueName + " does not have a binding of type " + typeof(T));
            return default(T);
        }

        /// <summary>
        /// Override for custom initialization (only used in the editor).
        /// </summary>
        public virtual void Initialize()
        {
            valueName = "";
            synonyms = "";
        }

        /// <summary>
        /// Returns the synonyms as a list of strings.
        /// </summary>
        public List<string> GetSynonymList()
        {
            List<string> list = new List<string>();

            foreach (string item in synonyms.Split(','))
            {
                string trimmed = item.Trim();

                if (trimmed.Length > 0)
                {
                    list.Add(trimmed);
                }
            }

            return list;
        }
    }
}
