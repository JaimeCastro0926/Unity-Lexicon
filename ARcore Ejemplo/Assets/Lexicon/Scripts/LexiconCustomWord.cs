// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class LexiconCustomWord
    {
        [SerializeField]
        private string word;

        [SerializeField]
        private string displayAs;

        [SerializeField]
        private string pronunciations;

        public string Word
        {
            get { return word; }
            set { word = value; }
        }

        public string DisplayAs
        {
            get { return displayAs; }
            set { displayAs = value; }
        }

        public string Pronunciations
        {
            get { return pronunciations; }
            set { pronunciations = value; }
        }

        public List<string> GetPronunciationList()
        {
            List<string> list = new List<string>();

            foreach (string item in pronunciations.Split(','))
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
