// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Material")]
    public class MaterialEntity : LexiconEntity
    {
        [SerializeField]
        private MaterialEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class MaterialEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private Material binding;

        public override object Binding
        {
            get { return binding; }
        }

        public override void Initialize()
        {
            base.Initialize();
            binding = null;
        }
    }
}
