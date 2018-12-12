// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/String")]
    public class StringEntity : LexiconEntity
    {
        [SerializeField]
        private StringEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class StringEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private string binding;

        public override object Binding
        {
            get { return binding; }
        }

        public override void Initialize()
        {
            base.Initialize();
            binding = "";
        }
    }
}
