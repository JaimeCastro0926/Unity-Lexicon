// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class ValidateEntity : SyncAction
    {
        private LexiconEntity entity;

        public static ValidateEntity CreateInstance(LexiconEntity entity)
        {
            ValidateEntity instance = CreateInstance<ValidateEntity>();
            instance.entity = entity;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (entity == null)
            {
                // Entity was deleted.
                succeeded = false;
                isDone = true;
                return;
            }

            succeeded = true;

            if (string.IsNullOrEmpty(entity.EntityName))
            {
                Debug.LogError("Entity name required");
                succeeded = false;
            }

            foreach (LexiconEntityValue entityValue in entity.Values)
            {
                if (entityValue == null)
                {
                    Debug.LogError("Entity has a null value");
                    succeeded = false;
                }
                else if (string.IsNullOrEmpty(entityValue.ValueName))
                {
                    Debug.LogError("Entity has an empty value name");
                    succeeded = false;
                }
            }

            isDone = true;
        }
    }
}

#endif
