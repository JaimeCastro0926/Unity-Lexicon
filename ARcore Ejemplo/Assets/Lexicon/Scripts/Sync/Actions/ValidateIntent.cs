// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class ValidateIntent : SyncAction
    {
        private LexiconIntent intent;

        public static ValidateIntent CreateInstance(LexiconIntent intent)
        {
            ValidateIntent instance = CreateInstance<ValidateIntent>();
            instance.intent = intent;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (intent == null)
            {
                // Intent was deleted.
                succeeded = false;
                isDone = true;
                return;
            }

            succeeded = true;

            if (string.IsNullOrEmpty(intent.IntentName))
            {
                Debug.LogError("Intent name required");
                succeeded = false;
            }

            if (string.IsNullOrEmpty(intent.ActionName))
            {
                Debug.LogError("Action name required");
                succeeded = false;
            }

            foreach (LexiconEntity entity in intent.RequiredEntities)
            {
                if (entity == null)
                {
                    Debug.LogError("Intent has a null entry in the Required Entities list");
                    succeeded = false;
                }
                else if (string.IsNullOrEmpty(entity.EntityName))
                {
                    Debug.LogError("Intent has a Required Entity with an empty name: " + entity.name);
                    succeeded = false;
                }
            }

            foreach (LexiconEntity entity in intent.OptionalEntities)
            {
                if (entity == null)
                {
                    Debug.LogError("Intent has a null entry in the Optional Entities list");
                    succeeded = false;
                }
                else if (string.IsNullOrEmpty(entity.EntityName))
                {
                    Debug.LogError("Intent has an Optional Entity with an empty name: " + entity.name);
                    succeeded = false;
                }
            }

            isDone = true;
        }
    }
}

#endif
