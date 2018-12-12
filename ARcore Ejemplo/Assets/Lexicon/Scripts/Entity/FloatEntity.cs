// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Float")]
    public class FloatEntity : LexiconEntity
    {
        [SerializeField]
        private FloatEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class FloatEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private float binding;

        public override object Binding
        {
            get { return binding; }
        }

        public override void Initialize()
        {
            base.Initialize();
            binding = 0.0f;
        }
    }
}
