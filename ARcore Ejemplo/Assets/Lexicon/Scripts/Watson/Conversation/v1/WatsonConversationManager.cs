// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using FullSerializer;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using Mixspace.Lexicon.Watson.Conversation.v1;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class WatsonConversationManager
    {
        [SerializeField]
        private string url = "https://gateway.watsonplatform.net/conversation/api";

        [SerializeField]
        private string language = "en";

        [SerializeField]
        private string workspaceId;

        [SerializeField]
        private string syncStatus;

        [SerializeField]
        private bool isSyncing;

        [SerializeField]
        private bool isTraining;

        [SerializeField]
        private string lastSyncTime;

        [SerializeField]
        private WorkspaceSyncData lastSyncData;

        [SerializeField]
        private WorkspaceTimestamps timestamps;

        [NonSerialized]
        private bool alternateIntents;

        public string Username
        {
            get
            {
                if (Application.isEditor)
                {
                    return StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonConversationUsername));
                }
                else
                {
                    LexiconConfig config = (LexiconConfig)Resources.Load(LexiconConstants.Files.ConfigFile);
                    return ((config != null) ? StringUtility.Base64Decode(config.WatsonConversationUsername) : "");
                }
            }
        }

        public string Password
        {
            get
            {
                if (Application.isEditor)
                {
                    return StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonConversationPassword));
                }
                else
                {
                    LexiconConfig config = (LexiconConfig)Resources.Load(LexiconConstants.Files.ConfigFile);
                    return ((config != null) ? StringUtility.Base64Decode(config.WatsonConversationPassword) : "");
                }
            }
        }

        public string Url
        {
            get { return url; }
        }

        public string Language
        {
            get { return language; }
        }

        public string WorkspaceId
        {
            get { return workspaceId; }
            set { workspaceId = value; }
        }

        public string SyncStatus
        {
            get { return syncStatus; }
            set { syncStatus = value; }
        }

        public bool IsSyncing
        {
            get { return isSyncing; }
            set { isSyncing = value; }
        }

        public bool IsTraining
        {
            get { return isTraining; }
            set { isTraining = value; }
        }

        public string LastSyncTime
        {
            get { return lastSyncTime; }
            set { lastSyncTime = value; }
        }

        public WorkspaceSyncData LastSyncData
        {
            get { return lastSyncData; }
            set { lastSyncData = value; }
        }

        public WorkspaceTimestamps Timestamps
        {
            get { return timestamps; }
            set { timestamps = value; }
        }

        public bool AlternateIntents
        {
            get { return alternateIntents; }
            set { alternateIntents = value; }
        }

        public LexiconConversation Conversation
        {
            get { return conversation; }
        }

        public delegate void OnConversationResponse(MessageResponse response);
        public OnConversationResponse ResponseHandlers;

        private LexiconConversation conversation;
        private string conversationVersionDate = "2017-05-26";
        private fsSerializer serializer = new fsSerializer();

        public void Initialize()
        {
            LogSystem.InstallDefaultReactors();

            Credentials credentials = new Credentials(Username, Password, url);

            conversation = new LexiconConversation(credentials);
            conversation.VersionDate = conversationVersionDate;
        }

        public void SendRequest(string request)
        {
            if (string.IsNullOrEmpty(workspaceId))
            {
                Debug.LogError("Conversation Workspace Id is null, please sync Watson Conversation");
                return;
            }

            if (!conversation.MessageWebRequest(OnMessage, HandleFailCallback, workspaceId, request))
            {
                Debug.LogError("Failed to send message to Conversation service");
            }
        }

        private void OnMessage(object resp, Dictionary<string, object> customData)
        {
            //Debug.Log("Conversation Response: " + customData["json"]);

            //  Convert resp to fsdata
            fsData fsdata = null;
            fsResult r = serializer.TrySerialize(resp.GetType(), resp, out fsdata);
            if (!r.Succeeded)
                throw new WatsonException(r.FormattedMessages);

            //  Convert fsdata to MessageResponse
            MessageResponse messageResponse = new MessageResponse();
            object obj = messageResponse;
            r = serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
            if (!r.Succeeded)
                throw new WatsonException(r.FormattedMessages);

            if (ResponseHandlers != null)
            {
                ResponseHandlers(messageResponse);
            }
        }

        void HandleFailCallback(RESTConnector.Error error, Dictionary<string, object> customData)
        {
            Debug.LogError(error);
        }
    }
}
