// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Workspace", order = -2)]
    public class LexiconWorkspace : ScriptableObject
    {
        [SerializeField]
        private string workspaceName;

        [SerializeField]
        private string workspaceDescription;

        [SerializeField]
        private bool useWatsonSpeechToText;

        [SerializeField]
        private WatsonSpeechToTextManager watsonSpeechToTextManager;

        [SerializeField]
        private bool useWatsonConversation;

        [SerializeField]
        private WatsonConversationManager watsonConversationManager;

        [SerializeField]
        private List<LexiconIntent> intents;

        [SerializeField]
        private List<LexiconCustomWord> customWords;

        public string WorkspaceName
        {
            get { return workspaceName; }
        }

        public string WorkspaceDescription
        {
            get { return workspaceDescription; }
        }

        public bool UseWatsonSpeechToText
        {
            get { return useWatsonSpeechToText; }
        }

        public WatsonSpeechToTextManager WatsonSpeechToTextManager
        {
            get { return watsonSpeechToTextManager; }
        }

        public bool UseWatsonConversation
        {
            get { return useWatsonConversation; }
        }

        public WatsonConversationManager WatsonConversationManager
        {
            get { return watsonConversationManager; }
        }

        public List<LexiconIntent> Intents
        {
            get { return intents; }
        }

        public List<LexiconCustomWord> CustomWords
        {
            get { return customWords; }
        }

        public string CanonicalName
        {
            get
            {
                return workspaceName.Replace(' ', '_');
            }
        }

        public bool UsesIntent(LexiconIntent intent)
        {
            return intents.Contains(intent);
        }

        public bool UsesEntity(LexiconEntity entity)
        {
            foreach (LexiconIntent intent in intents)
            {
                if (intent.UsesEntity(entity))
                {
                    return true;
                }
            }

            return false;
        }

        public List<string> GetUniqueIntentNames()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> list = new List<string>();

            foreach (LexiconIntent intent in intents)
            {
                if (set.Add(intent.IntentName)) list.Add(intent.IntentName);
            }

            return list;
        }

        public List<string> GetUniqueEventNames()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> list = new List<string>();

            foreach (LexiconIntent intent in intents)
            {
                if (set.Add(intent.ActionName)) list.Add(intent.ActionName);
            }

            return list;
        }

        public List<LexiconEntity> GetUniqueEntities()
        {
            HashSet<LexiconEntity> set = new HashSet<LexiconEntity>();
            List<LexiconEntity> list = new List<LexiconEntity>();

            foreach (LexiconIntent intent in intents)
            {
                foreach (LexiconEntity entity in intent.RequiredEntities)
                {
                    if (set.Add(entity)) list.Add(entity);
                }

                foreach (LexiconEntity entity in intent.OptionalEntities)
                {
                    if (set.Add(entity)) list.Add(entity);
                }
            }

            return list;
        }

        public List<string> GetUniqueEntityNames()
        {
            List<LexiconEntity> entities = GetUniqueEntities();

            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> list = new List<string>();

            foreach (LexiconEntity entity in entities)
            {
                if (set.Add(entity.EntityName)) list.Add(entity.EntityName);
            }

            return list;
        }

        public List<string> GetUniqueEntityValues(string entityName)
        {
            List<LexiconEntity> entities = GetUniqueEntities();

            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> list = new List<string>();

            foreach (LexiconEntity entity in entities)
            {
                if (entityName.Equals(entity.EntityName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (LexiconEntityValue entityValue in entity.Values)
                    {
                        if (set.Add(entityValue.ValueName)) list.Add(entityValue.ValueName);
                    }
                }
            }

            return list;
        }

        public LexiconEntityValue GetEntityValue(string entityName, string valueName)
        {
            // TODO: create a hashtable of entity names
            // TODO: this doesn't actually work, as multiple entities can exist with the same name and values (we need the context of the intent to pick one)

            List<LexiconEntity> entities = GetUniqueEntities();

            foreach (LexiconEntity entity in entities)
            {
                if (entityName.Equals(entity.EntityName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (LexiconEntityValue value in entity.Values)
                    {
                        if (valueName.Equals(value.ValueName, StringComparison.OrdinalIgnoreCase))
                        {
                            return value;
                        }
                    }
                }
            }

            return null;
        }

        public List<LexiconIntent> FindMatchingIntents(string intentName, List<EntityPair> entityPairs)
        {
            List<LexiconIntent> matchingIntents = new List<LexiconIntent>();

            foreach (LexiconIntent intent in intents)
            {
                if (intentName.Equals(intent.IntentName, StringComparison.OrdinalIgnoreCase))
                {
                    if (intent.IsSatisfied(entityPairs))
                    {
                        matchingIntents.Add(intent);
                    }
                }
            }

            return matchingIntents;
        }
    }
}
 