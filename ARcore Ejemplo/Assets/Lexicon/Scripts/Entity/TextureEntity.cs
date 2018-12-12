﻿// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Texture")]
    public class TextureEntity : LexiconEntity
    {
        [SerializeField]
        private TextureEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class TextureEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private Texture binding;

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
