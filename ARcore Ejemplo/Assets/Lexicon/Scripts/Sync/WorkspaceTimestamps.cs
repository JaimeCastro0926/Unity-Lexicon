// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WorkspaceTimestamps : ISerializationCallbackReceiver
    {
        private Dictionary<string, string> intentTimestamps;

        private Dictionary<string, string> entityTimestamps;

        private Dictionary<string, LexiconEntity> syncedEntities;

        public List<string> GetAllSyncedIntents()
        {
            return new List<string>(intentTimestamps.Keys);
        }

        public List<string> GetAllSyncedEntities()
        {
            return new List<string>(entityTimestamps.Keys);
        }

        public string GetIntentTimestamp(string intent)
        {
            if (intent != null)
            {
                string timestamp;

                if (intentTimestamps.TryGetValue(intent, out timestamp))
                {
                    return timestamp;
                }
            }

            return null;
        }

        public string GetEntityTimestamp(string entity)
        {
            if (entity != null)
            {
                string timestamp;

                if (entityTimestamps.TryGetValue(entity, out timestamp))
                {
                    return timestamp;
                }
            }

            return null;
        }

        public void SetIntentTimestamp(string intent, string timestamp)
        {
            intentTimestamps[intent] = timestamp;
            timestampsAreDirty = true;
        }

        public void SetEntityTimestamp(string entity, string timestamp)
        {
            entityTimestamps[entity] = timestamp;
            timestampsAreDirty = true;
        }

        public void RemoveIntentTimestamp(string intent)
        {
            intentTimestamps.Remove(intent);
            timestampsAreDirty = true;
        }

        public void RemoveEntityTimestamp(string entity)
        {
            entityTimestamps.Remove(entity);
            timestampsAreDirty = true;
        }


        // Handle the fact that Unity can't serialize Dictionaries

        [SerializeField]
        private List<string> intentTimestampKeys = new List<string>();

        [SerializeField]
        private List<string> intentTimestampValues = new List<string>();

        [SerializeField]
        private List<string> entityTimestampKeys = new List<string>();

        [SerializeField]
        private List<string> entityTimestampValues = new List<string>();

        private bool timestampsAreDirty;

        public void OnAfterDeserialize()
        {
            //Debug.Log("ConversationTimestamps OnAfterDeserialize");

            intentTimestamps = new Dictionary<string, string>();
            entityTimestamps = new Dictionary<string, string>();

            for (int i = 0; i < Math.Min(intentTimestampKeys.Count, intentTimestampValues.Count); i++)
            {
                intentTimestamps.Add(intentTimestampKeys[i], intentTimestampValues[i]);
            }

            for (int i = 0; i < Math.Min(entityTimestampKeys.Count, entityTimestampValues.Count); i++)
            {
                entityTimestamps.Add(entityTimestampKeys[i], entityTimestampValues[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            if (timestampsAreDirty)
            {
                //Debug.Log("ConversationTimestamps OnBeforeSerialize");

                intentTimestampKeys.Clear();
                intentTimestampValues.Clear();

                entityTimestampKeys.Clear();
                entityTimestampValues.Clear();

                foreach (KeyValuePair<string, string> kvp in intentTimestamps)
                {
                    intentTimestampKeys.Add(kvp.Key);
                    intentTimestampValues.Add(kvp.Value);
                }

                foreach (KeyValuePair<string, string> kvp in entityTimestamps)
                {
                    entityTimestampKeys.Add(kvp.Key);
                    entityTimestampValues.Add(kvp.Value);
                }

                timestampsAreDirty = false;
            }
        }
    }
}
