using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Entity/Boolean")]
    public class BoolEntity : LexiconEntity
    {
        [SerializeField]
        private BoolEntityValue[] values;

        public override LexiconEntityValue[] Values
        {
            get { return values; }
        }
    }

    [Serializable]
    public class BoolEntityValue : LexiconEntityValue
    {
        [SerializeField]
        private bool binding;

        public override object Binding
        {
            get { return binding; }
        }

        public override void Initialize()
        {
            base.Initialize();
            binding = false; // or true!
        }
    }
}
