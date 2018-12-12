// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Prefab")]
    public class PrefabEntity : LexiconEntity
    {
        [SerializeField]
        private PrefabEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class PrefabEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private GameObject binding;

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
