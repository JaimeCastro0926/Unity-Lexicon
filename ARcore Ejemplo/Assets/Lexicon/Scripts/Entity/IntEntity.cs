// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Int")]
    public class IntEntity : LexiconEntity
    {
        [SerializeField]
        private IntEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class IntEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private int binding;

        public override object Binding
        {
            get { return binding; }
        }

        public override void Initialize()
        {
            base.Initialize();
            binding = 0;
        }
    }
}
