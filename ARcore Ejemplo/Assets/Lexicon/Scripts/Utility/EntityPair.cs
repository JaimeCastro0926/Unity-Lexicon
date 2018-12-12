// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

namespace Mixspace.Lexicon
{
    /// <summary>
    /// Simple class for passing around entity name/value pairs.
    /// </summary>
    public class EntityPair
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public EntityPair(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
