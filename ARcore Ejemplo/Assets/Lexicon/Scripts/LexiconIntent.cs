// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [CreateAssetMenu(menuName = "Lexicon/Intent", order = -1)]
    public class LexiconIntent : ScriptableObject
    {
        [SerializeField]
        private string intentName;

        [SerializeField]
        private string actionName;

        [SerializeField]
        private string namespaceString;

        [SerializeField]
        private LexiconAction defaultAction;

        [SerializeField]
        private List<LexiconEntity> requiredEntities; // TODO: Change these to arrays.

        [SerializeField]
        private List<LexiconEntity> optionalEntities;

        [SerializeField]
        private string[] phrases;

        private LexiconAction actionSingleton;

        /// <summary>
        /// The name of the intent (e.g. Create).
        /// </summary>
        /// <remarks>
        /// These do not need to be unique, use actionName to differentiate similar intents.
        /// </remarks>
        public string IntentName
        {
            get { return intentName; }
        }

        /// <summary>
        /// The name of the action for this intent (e.g. Create Primitive).
        /// </summary>
        /// <remarks>
        /// Used to differentiate similar intents (e.g. Create Primitive vs. Create Model).
        /// </remarks>
        public string ActionName
        {
            get { return actionName; }
        }

        /// <summary>
        /// Namespace for the generated string and action classes.
        /// </summary>
        public string NamespaceString
        {
            get
            {
                if (string.IsNullOrEmpty(namespaceString))
                {
                    return "Mixspace.Lexicon.Actions";
                }
                else
                {
                    return namespaceString;
                }
            }
        }

        /// <summary>
        /// The action that will be executed when this intent is matched.
        /// </summary>
        public LexiconAction DefaultAction
        {
            get { return defaultAction; }
            set { defaultAction = value; }
        }

        /// <summary>
        /// A list of entities that must be satisfied for this intent to be matched.
        /// </summary>
        public List<LexiconEntity> RequiredEntities
        {
            get { return requiredEntities; }
        }

        /// <summary>
        /// A list of optional entities.
        /// </summary>
        public List<LexiconEntity> OptionalEntities
        {
            get { return optionalEntities; }
        }

        /// <summary>
        /// A list of sample phrases used to train this intent.
        /// </summary>
        public string[] Phrases
        {
            get { return phrases; }
        }

        /// <summary>
        /// Returns a list of all required and optional entities.
        /// </summary>
        public List<LexiconEntity> GetAllEntities()
        {
            List<LexiconEntity> entities = new List<LexiconEntity>(requiredEntities);
            entities.AddRange(optionalEntities);
            return entities;
        }

        /// <summary>
        /// Returns true if the given entity is used by this intent.
        /// </summary>
        public bool UsesEntity(LexiconEntity entity)
        {
            return requiredEntities.Contains(entity) || optionalEntities.Contains(entity);
        }

        /// <summary>
        /// Returns true if the given list of entity name/value pairs satisfies this intent.
        /// </summary>
        public bool IsSatisfied(List<EntityPair> entityPairs)
        {
            foreach (LexiconEntity entity in requiredEntities)
            {
                if (entity == null)
                {
                    continue;
                }

                bool found = false;
                foreach (EntityPair entityPair in entityPairs)
                {
                    if (entity.EntityName.Equals(entityPair.Name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (entity is SystemEntity)
                        {
                            found = true;
                        }
                        else if (entity.FindValueByName(entityPair.Value) != null)
                        {
                            found = true;
                        }
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns an entity value matching the given name and value.
        /// </summary>
        public LexiconEntityValue FindEntityValue(string entityName, string valueName)
        {
            LexiconEntity entity;
            return FindEntityValue(entityName, valueName, out entity);
        }

        /// <summary>
        /// Returns an entity value matching the given name and value, with returnEntity containing the matched entity.
        /// </summary>
        public LexiconEntityValue FindEntityValue(string entityName, string valueName, out LexiconEntity returnEntity)
        {
            foreach (LexiconEntity entity in requiredEntities)
            {
                if (entity == null)
                {
                    continue;
                }

                if (entityName.Equals(entity.EntityName, System.StringComparison.OrdinalIgnoreCase))
                {
                    LexiconEntityValue entityValue = entity.FindValueByName(valueName);
                    if (entityValue != null)
                    {
                        returnEntity = entity;
                        return entityValue;
                    }
                }
            }

            foreach (LexiconEntity entity in optionalEntities)
            {
                if (entity == null)
                {
                    continue;
                }

                if (entityName.Equals(entity.EntityName, System.StringComparison.OrdinalIgnoreCase))
                {
                    LexiconEntityValue entityValue = entity.FindValueByName(valueName);
                    if (entityValue != null)
                    {
                        returnEntity = entity;
                        return entityValue;
                    }
                }
            }

            returnEntity = null;
            return null;
        }

        public bool Process(LexiconRuntimeResult runtimeResult)
        {
            if (defaultAction != null)
            {
                if (actionSingleton != null)
                {
                    // Use the existing action instance
                    return actionSingleton.Process(runtimeResult);
                }
                else
                {
                    // Create a new action instance
                    LexiconAction handlerObject = Instantiate(defaultAction);
                    bool consumed = handlerObject.Process(runtimeResult);

                    if (handlerObject.DestroyOnComplete)
                    {
                        Destroy(handlerObject.gameObject);
                    }
                    else if (handlerObject.Singleton)
                    {
                        actionSingleton = handlerObject;
                        actionSingleton.OnDestroyAction += OnDestroyAction;
                    }

                    return consumed;
                }
            }

            return false;
        }

        void OnDestroyAction()
        {
            actionSingleton = null;
        }
    }
}
