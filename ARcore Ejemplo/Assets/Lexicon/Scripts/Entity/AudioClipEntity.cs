// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/AudioClip")]
    public class AudioClipEntity : LexiconEntity
    {
        [SerializeField]
        private AudioClipEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class AudioClipEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private AudioClip binding;

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
