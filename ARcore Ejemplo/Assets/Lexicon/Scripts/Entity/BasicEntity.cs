// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Basic", order = -1)]
    public class BasicEntity : LexiconEntity
    {
        [SerializeField]
        private LexiconEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }
}
