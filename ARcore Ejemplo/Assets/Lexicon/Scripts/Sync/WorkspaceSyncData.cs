// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WorkspaceSyncData : ISerializationCallbackReceiver
    {
        public Dictionary<string, IntentData> intentData = new Dictionary<string, IntentData>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, EntityData> entityData = new Dictionary<string, EntityData>(StringComparer.OrdinalIgnoreCase);

        //public Dictionary<string, LexiconCustomWord> customWords = new Dictionary<string, LexiconCustomWord>();

        public List<LexiconCustomWord> customWords = new List<LexiconCustomWord>();

        public static WorkspaceSyncData Create(LexiconWorkspace workspace)
        {
            WorkspaceSyncData syncData = new WorkspaceSyncData();

            foreach (LexiconIntent intent in workspace.Intents)
            {
                if (string.IsNullOrEmpty(intent.IntentName))
                {
                    Debug.LogError("Found intent with empty name, skipping");
                    continue;
                }

                IntentData intentData;
                if (syncData.intentData.TryGetValue(intent.IntentName, out intentData))
                {
                    // Intent exists, add any new phrases
                    foreach (string phrase in intent.Phrases)
                    {
                        if (intentData.phrases.FindIndex(x => x.Equals(phrase, StringComparison.OrdinalIgnoreCase)) < 0)
                        {
                            // Phrase does not exist, add it
                            intentData.phrases.Add(phrase);
                        }
                    }
                }
                else
                {
                    // Intent does not exist, add it
                    intentData = new IntentData();
                    intentData.name = intent.IntentName;
                    intentData.phrases.AddRange(intent.Phrases);
                    syncData.intentData.Add(intent.IntentName, intentData);
                }
            }

            foreach (LexiconEntity entity in workspace.GetUniqueEntities())
            {
                if (entity == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entity.EntityName))
                {
                    Debug.LogError("Found entity with empty name, skipping");
                    continue;
                }

                EntityData entityData;
                if (syncData.entityData.TryGetValue(entity.EntityName, out entityData))
                {
                    // Entity exists, add any new values
                    foreach (LexiconEntityValue entityValue in entity.Values)
                    {
                        List<string> synonyms = entityValue.GetSynonymList();
                        int valueIndex = entityData.values.FindIndex(x => x.name.Equals(entityValue.ValueName, StringComparison.OrdinalIgnoreCase));
                        if (valueIndex < 0)
                        {
                            // Value does not exist, add it
                            EntityValueData entityValueData = new EntityValueData();
                            entityValueData.name = entityValue.ValueName;
                            entityValueData.synonyms.AddRange(synonyms);
                            entityData.values.Add(entityValueData);
                        }
                        else
                        {
                            // Value exists, add any new synonyms
                            EntityValueData entityValueData = entityData.values[valueIndex];
                            foreach (string synonym in synonyms)
                            {
                                if (entityValueData.synonyms.FindIndex(x => x.Equals(synonym, StringComparison.OrdinalIgnoreCase)) < 0)
                                {
                                    // Synonym does not exist, add it
                                    entityValueData.synonyms.Add(synonym);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Entity does not exist, add it
                    entityData = new EntityData();
                    entityData.name = entity.EntityName;
                    if (entity.Values != null)
                    {
                        foreach (LexiconEntityValue entityValue in entity.Values)
                        {
                            List<string> synonyms = entityValue.GetSynonymList();
                            EntityValueData entityValueData = new EntityValueData();
                            entityValueData.name = entityValue.ValueName;
                            entityValueData.synonyms.AddRange(synonyms);
                            entityData.values.Add(entityValueData);
                        }
                    }
                    syncData.entityData.Add(entity.EntityName, entityData);
                }
            }

            syncData.customWords.AddRange(workspace.CustomWords);

            //foreach (LexiconCustomWord customWord in workspace.customWords)
            //{
            //    syncData.customWords.Add(customWord.word, customWord);
            //}

            syncData.dataIsDirty = true;

            return syncData;
        }

        public static bool CompareIntentData(IntentData intent1, IntentData intent2)
        {
            if (!intent1.name.Equals(intent2.name))
            {
                return false;
            }

            if (intent1.phrases.Count != intent2.phrases.Count)
            {
                return false;
            }

            foreach (string phrase in intent1.phrases)
            {
                if (!intent2.phrases.Contains(phrase))
                {
                    return false;
                }
            }

            foreach (string phrase in intent2.phrases)
            {
                if (!intent1.phrases.Contains(phrase))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CompareEntityData(EntityData entity1, EntityData entity2)
        {
            if (!entity1.name.Equals(entity2.name))
            {
                return false;
            }

            if (entity1.values.Count != entity2.values.Count)
            {
                return false;
            }

            foreach (EntityValueData value1 in entity1.values)
            {
                bool foundMatch = false;

                foreach (EntityValueData value2 in entity2.values)
                {
                    if (value1.name.Equals(value2.name))
                    {
                        if (CompareEntityValue(value1, value2))
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                }

                if (!foundMatch)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CompareEntityValue(EntityValueData value1, EntityValueData value2)
        {
            if (!value1.name.Equals(value2.name))
            {
                return false;
            }

            if (value1.synonyms.Count != value2.synonyms.Count)
            {
                return false;
            }

            foreach (string synonym in value1.synonyms)
            {
                if (!value2.synonyms.Contains(synonym))
                {
                    return false;
                }
            }

            foreach (string synonym in value2.synonyms)
            {
                if (!value1.synonyms.Contains(synonym))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CompareCustomWords(LexiconCustomWord word1, LexiconCustomWord word2)
        {
            if (!word1.Word.Equals(word2.Word))
            {
                return false;
            }

            if (!word1.DisplayAs.Equals(word2.DisplayAs))
            {
                return false;
            }

            if (!word1.Pronunciations.Equals(word2.Pronunciations))
            {
                return false;
            }

            return true;
        }

        // Handle the fact that Unity can't serialize Dictionaries

        [SerializeField]
        private List<string> intentDataKeys = new List<string>();

        [SerializeField]
        private List<IntentData> intentDataValues = new List<IntentData>();

        [SerializeField]
        private List<string> entityDataKeys = new List<string>();

        [SerializeField]
        private List<EntityData> entityDataValues = new List<EntityData>();

        private bool dataIsDirty;

        public void OnAfterDeserialize()
        {
            //Debug.Log("WorkspaceSyncData OnAfterDeserialize");

            intentData = new Dictionary<string, IntentData>();
            entityData = new Dictionary<string, EntityData>();

            for (int i = 0; i < Math.Min(intentDataKeys.Count, intentDataValues.Count); i++)
            {
                intentData.Add(intentDataKeys[i], intentDataValues[i]);
            }

            for (int i = 0; i < Math.Min(entityDataKeys.Count, entityDataValues.Count); i++)
            {
                entityData.Add(entityDataKeys[i], entityDataValues[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            if (dataIsDirty)
            {
                //Debug.Log("WorkspaceSyncData OnBeforeSerialize");

                intentDataKeys.Clear();
                intentDataValues.Clear();

                entityDataKeys.Clear();
                entityDataValues.Clear();

                foreach (KeyValuePair<string, IntentData> kvp in intentData)
                {
                    intentDataKeys.Add(kvp.Key);
                    intentDataValues.Add(kvp.Value);
                }

                foreach (KeyValuePair<string, EntityData> kvp in entityData)
                {
                    entityDataKeys.Add(kvp.Key);
                    entityDataValues.Add(kvp.Value);
                }

                dataIsDirty = false;
            }
        }
    }

    [Serializable]
    public class IntentData
    {
        public string name;
        public List<string> phrases = new List<string>();
    }

    [Serializable]
    public class EntityData
    {
        public string name;
        public List<EntityValueData> values = new List<EntityValueData>();
    }

    [Serializable]
    public class EntityValueData
    {
        public string name;
        public List<string> synonyms = new List<string>();
    }
}
