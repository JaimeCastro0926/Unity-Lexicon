// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Color")]
    public class ColorEntity : LexiconEntity
    {
        [SerializeField]
        private ColorEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class ColorEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private Color binding;

        public override object Binding
        {
            get { return binding; }
        }

        public override void Initialize()
        {
            base.Initialize();
            binding = Color.white;
        }
    }
}
