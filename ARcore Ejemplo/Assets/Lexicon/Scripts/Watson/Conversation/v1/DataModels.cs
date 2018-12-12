// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using FullSerializer;

namespace Mixspace.Lexicon.Watson.Conversation.v1
{
    [fsObject]
    public class Workspace
    {
        /// <summary>
        /// The name of the workspace.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The language of the workspace.
        /// </summary>
        public string language { get; set; }
        /// <summary>
        /// The timestamp for creation of the workspace.
        /// </summary>
        public string created { get; set; }
        /// <summary>
        /// The timestamp for the last update to the workspace.
        /// </summary>
        public string updated { get; set; }
        /// <summary>
        /// The workspace ID.
        /// </summary>
        public string workspace_id { get; set; }
        /// <summary>
        /// The description of the workspace.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Whether training data from the workspace can be used by IBM for general service improvements. Training data includes artifacts such as intents and entities. true indicates that workspace training data is not to be used.
        /// </summary>
        public bool learning_opt_out { get; set; }
        /// <summary>
        /// The current status of the workspace (Non Existent, Training, Failed, Available, or Unavailable).
        /// </summary>
        public string status { get; set; }
    }

    [fsObject]
    public class Intent
    {
        /// <summary>
        /// The name of the intent.
        /// </summary>
        public string intent { get; set; }
        /// <summary>
        /// The timestamp for creation of the intent.
        /// </summary>
        public string created { get; set; }
        /// <summary>
        /// The timestamp for the last update to the intent.
        /// </summary>
        public string updated { get; set; }
        /// <summary>
        /// The description of the intent.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// An array of objects describing the user input examples for the intent.
        /// </summary>
        public Example[] examples { get; set; }
    }

    [fsObject]
    public class Example
    {
        /// <summary>
        /// The text of the user input example.
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// The timestamp for creation of the user input example.
        /// </summary>
        public string created { get; set; }
        /// <summary>
        /// The timestamp for the last update to the user input example.
        /// </summary>
        public string updated { get; set; }
    }

    [fsObject]
    public class Entity
    {
        /// <summary>
        /// The name of the entity (for example, beverage).
        /// </summary>
        public string entity { get; set; }
        /// <summary>
        /// The timestamp for creation of the entity.
        /// </summary>
        public string created { get; set; }
        /// <summary>
        /// The timestamp for the last update to the entity.
        /// </summary>
        public string updated { get; set; }
        /// <summary>
        /// The description of the entity.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Whether fuzzy matching is used for the entity.
        /// </summary>
        public bool fuzzy_match { get; set; }
        /// <summary>
        /// An array of objects describing the entity values.
        /// </summary>
        public ValueExport[] values { get; set; }
    }

    [fsObject]
    public class EntityValue
    {
        /// <summary>
        /// The text of the entity value.
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// The timestamp for the creation of the entity value.
        /// </summary>
        public string created { get; set; }
        /// <summary>
        /// The timestamp for the last update to the entity value.
        /// </summary>
        public string updated { get; set; }
    }

    [fsObject]
    public class ValueExport
    {
        /// <summary>
        /// The text of the entity value.
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// The timestamp for the creation of the entity value.
        /// </summary>
        public string created { get; set; }
        /// <summary>
        /// The timestamp for the last update to the entity value.
        /// </summary>
        public string updated { get; set; }
        /// <summary>
        /// An array containing any synonyms for the entity value.
        /// </summary>
        public string[] synonyms { get; set; }
    }

    [fsObject]
    public class CreateWorkspace
    {
        /// <summary>
        /// The name of the workspace.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The description of the workspace.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// The language of the workspace.
        /// </summary>
        public string language { get; set; }
        /// <summary>
        /// Whether training data from the workspace can be used by IBM for general service improvements. Training data includes artifacts such as intents and entities. true indicates that workspace training data is not to be used.
        /// </summary>
        public bool learning_opt_out { get; set; }
    }

    [fsObject]
    public class CreateIntent
    {
        /// <summary>
        /// The name of the intent.
        /// </summary>
        public string intent { get; set; }
        /// <summary>
        /// The description of the intent.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// An array of user input examples for the intent.
        /// </summary>
        public CreateExample[] examples { get; set; }
    }

    [fsObject]
    public class UpdateIntent
    {
        /// <summary>
        /// The name of the intent.
        /// </summary>
        public string intent { get; set; }
        /// <summary>
        /// The description of the intent.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// An array of user input examples for the intent.
        /// </summary>
        public CreateExample[] examples { get; set; }
    }

    [fsObject]
    public class CreateExample
    {
        /// <summary>
        /// The text of a user input example.
        /// </summary>
        public string text { get; set; }
    }

    [fsObject]
    public class CreateEntity
    {
        /// <summary>
        /// The name of the entity (for example, beverage).
        /// </summary>
        public string entity { get; set; }
        /// <summary>
        /// The description of the entity.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// An array of entity values.
        /// </summary>
        public CreateValue[] values { get; set; }
        /// <summary>
        /// Whether to use fuzzy matching for the entity.
        /// </summary>
        public bool fuzzy_match { get; set; }
    }

    [fsObject]
    public class UpdateEntity
    {
        /// <summary>
        /// The name of the entity (for example, beverage).
        /// </summary>
        public string entity { get; set; }
        /// <summary>
        /// The description of the entity.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Whether to use fuzzy matching for the entity.
        /// </summary>
        public bool fuzzy_match { get; set; }
        /// <summary>
        /// An array of entity values.
        /// </summary>
        public CreateValue[] values { get; set; }
    }

    [fsObject]
    public class CreateValue
    {
        /// <summary>
        /// The text of the entity value.
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// An array containing any synonyms for the entity value.
        /// </summary>
        public string[] synonyms { get; set; }
    }

    [fsObject]
    public class ListIntentsResp
    {
        /// <summary>
        /// An array of IntentExport objects describing the intents defined for the workspace.
        /// </summary>
        public Intent[] intents { get; set; }
        /// <summary>
        /// A Pagination object defining the pagination data for the returned objects.
        /// </summary>
        public Pagination pagination { get; set; }
    }

    [fsObject]
    public class ListEntitiesResp
    {
        /// <summary>
        /// An array of EntityExport objects describing the entities defined for the workspace.
        /// </summary>
        public Entity[] entities { get; set; }
        /// <summary>
        /// A Pagination object defining the pagination data for the returned objects.
        /// </summary>
        public Pagination pagination { get; set; }
    }

    [fsObject]
    public class Pagination
    {
        /// <summary>
        /// The URL that will return the same page of results.
        /// </summary>
        public string refresh_url { get; set; }
        /// <summary>
        /// The URL that will return the next page of results, if any.
        /// </summary>
        public string next_url { get; set; }
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public int matched { get; set; }
    }
}
