// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;

namespace Mixspace.Lexicon
{
    public class SystemEntity : LexiconEntity
    {
        public override LexiconEntityValue[] Values
        {
            get { return null; }
        }

        public override LexiconEntityValue FindValueByName(string name, bool searchSynonyms = false)
        {
            return new SystemEntityValue(name);
        }
    }

    public class SystemEntityValue : LexiconEntityValue
    {
        private string stringValue;
        private float floatValue;
        private int intValue;
        private DateTime dateTimeValue;

        public string StringValue
        {
            get { return stringValue; }
        }

        public float FloatValue
        {
            get { return floatValue; }
        }

        public int IntValue
        {
            get { return intValue; }
        }

        public DateTime DateTimeValue
        {
            get { return dateTimeValue; }
        }

        public SystemEntityValue(string value)
        {
            ValueName = value;

            stringValue = value;

            if (float.TryParse(value, out floatValue))
            {
                intValue = (int)floatValue;
            }

            DateTime.TryParse(value, out dateTimeValue);
        }
    }
}
