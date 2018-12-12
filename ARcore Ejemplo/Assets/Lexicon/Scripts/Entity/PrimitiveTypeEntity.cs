// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/PrimitiveType")]
    public class PrimitiveTypeEntity : LexiconEntity
    {
        [SerializeField]
        private PrimitiveTypeEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class PrimitiveTypeEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private PrimitiveType binding;

        public override object Binding
        {
            get { return binding; }
        }
    }
}
