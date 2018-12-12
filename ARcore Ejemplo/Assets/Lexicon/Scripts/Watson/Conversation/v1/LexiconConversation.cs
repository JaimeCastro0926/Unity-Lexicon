// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FullSerializer;
using IBM.Watson.DeveloperCloud.Connection;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using MiniJSON;

namespace Mixspace.Lexicon.Watson.Conversation.v1
{
    public class LexiconConversation : IBM.Watson.DeveloperCloud.Services.Conversation.v1.Conversation
    {
        private fsSerializer _serializer = new fsSerializer();

        #region Constructor
        public LexiconConversation(Credentials credentials) : base(credentials) { }
        #endregion

        #region Message
        public bool MessageWebRequest(SuccessCallback<object> successCallback, FailCallback failCallback, string workspaceID, string input, Dictionary<string, object> customData = null)
        {
            string reqJson = "{{\"input\": {{\"text\": \"{0}\"}}}}";
            string reqString = string.Format(reqJson, input);
            byte[] reqBytes = Encoding.UTF8.GetBytes(reqString);

            string function = "/v1/workspaces/" + workspaceID + "/message";
            string args = "?version=" + WWW.EscapeURL(VersionDate);

            string finalUrl = Url + function + args;
            
            UnityWebRequest request = new UnityWebRequest(finalUrl);
            request.method = "POST";
            request.uploadHandler = new UploadHandlerRaw(reqBytes);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            if (Credentials.HasWatsonAuthenticationToken())
            {
                request.SetRequestHeader(RESTConnector.AUTHENTICATION_TOKEN_AUTHORIZATION_HEADER, Credentials.WatsonAuthenticationToken);
            }
            else if (Credentials.HasCredentials())
            {
                request.SetRequestHeader(RESTConnector.AUTHENTICATION_AUTHORIZATION_HEADER, Credentials.CreateAuthorization());
            }

            request.SetRequestHeader("User-Agent", Constants.String.Version);

            request.chunkedTransfer = false;

            Runnable.Run(SendWebRequest(request, successCallback, failCallback));

            return true;
        }

        IEnumerator SendWebRequest(UnityWebRequest request, SuccessCallback<object> successCallback, FailCallback failCallback)
        {
            
#if UNITY_2017_2_OR_NEWER
            yield return request.SendWebRequest();
#else
            yield return request.Send();
#endif

            bool success = false;

            object result = null;
            string data = "";
            Dictionary<string, object> customData = new Dictionary<string, object>();

            // Note that request.isNetworkError and request.isHttpError always return true in Unity 2017.3 (UWP with .NET backend)

            if (request.responseCode == 200)
            {
                success = true;

                try
                {
                    data = request.downloadHandler.text;
                    result = Json.Deserialize(data);
                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation.MessageResp()", "MessageResp Exception: {0}", e.ToString());
                    data = e.Message;
                    success = false;
                }
            }

            if (success)
            {
                if (successCallback != null)
                {
                    successCallback(result, customData);
                }
            }
            else
            {
                if (failCallback != null)
                {
                    RESTConnector.Error error = new RESTConnector.Error()
                    {
                        URL = Url,
                        ErrorCode = request.responseCode,
                        ErrorMessage = request.error,
                        Response = request.downloadHandler.text
                    };

                    failCallback(error, null);
                }
            }
        }
        #endregion

        #region Create Workspace

        public bool CreateWorkspace(SuccessCallback<Workspace> successCallback, FailCallback failCallback, string name, string language = "en", string description = default(string), Dictionary<string, object> customData = null)
        {
            CreateWorkspace createWorkspace = new CreateWorkspace();
            createWorkspace.name = name;
            createWorkspace.language = language;
            createWorkspace.description = string.IsNullOrEmpty(description) ? name : description;

            fsData data;
            _serializer.TrySerialize(createWorkspace.GetType(), createWorkspace, out data).AssertSuccessWithoutWarnings();
            string createWorkspaceJson = fsJsonPrinter.CompressedJson(data);

            Debug.Log(createWorkspaceJson);

            CreateWorkspaceRequest req = new CreateWorkspaceRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Send = Encoding.UTF8.GetBytes(createWorkspaceJson);
            req.OnResponse = OnCreateWorkspaceResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class CreateWorkspaceRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Workspace> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnCreateWorkspaceResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Workspace workspace = new Workspace();
            fsData data = null;
            Dictionary<string, object> customData = ((CreateWorkspaceRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = workspace;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "CreateWorkspace Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((CreateWorkspaceRequest)req).SuccessCallback != null)
                    ((CreateWorkspaceRequest)req).SuccessCallback(workspace, customData);
            }
            else
            {
                if (((CreateWorkspaceRequest)req).FailCallback != null)
                    ((CreateWorkspaceRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Get Workspace

        public bool GetWorkspace(SuccessCallback<Workspace> successCallback, FailCallback failCallback, string workspaceID, bool export, Dictionary<string, object> customData = null)
        {
            GetWorkspaceRequest req = new GetWorkspaceRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Parameters["export"] = export ? "true" : "false";
            req.Function = "/" + workspaceID;
            req.OnResponse = OnGetWorkspaceResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class GetWorkspaceRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Workspace> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnGetWorkspaceResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Workspace workspace = new Workspace();
            fsData data = null;
            Dictionary<string, object> customData = ((GetWorkspaceRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = workspace;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "GetWorkspace Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((GetWorkspaceRequest)req).SuccessCallback != null)
                    ((GetWorkspaceRequest)req).SuccessCallback(workspace, customData);
            }
            else
            {
                if (((GetWorkspaceRequest)req).FailCallback != null)
                    ((GetWorkspaceRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region List Intents

        public bool ListIntents(SuccessCallback<ListIntentsResp> successCallback, FailCallback failCallback, string workspaceID, bool export, Dictionary<string, object> customData = null)
        {
            ListIntentsRequest req = new ListIntentsRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Parameters["export"] = export ? "true" : "false";
            req.Function = "/" + workspaceID + "/intents";
            req.OnResponse = OnListIntentsResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class ListIntentsRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<ListIntentsResp> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnListIntentsResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            ListIntentsResp listIntentsResp = new ListIntentsResp();
            fsData data = null;
            Dictionary<string, object> customData = ((ListIntentsRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = listIntentsResp;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "ListIntents Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((ListIntentsRequest)req).SuccessCallback != null)
                    ((ListIntentsRequest)req).SuccessCallback(listIntentsResp, customData);
            }
            else
            {
                if (((ListIntentsRequest)req).FailCallback != null)
                    ((ListIntentsRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region List Entities

        public bool ListEntities(SuccessCallback<ListEntitiesResp> successCallback, FailCallback failCallback, string workspaceID, bool export, Dictionary<string, object> customData = null)
        {
            ListEntitiesRequest req = new ListEntitiesRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Parameters["export"] = export ? "true" : "false";
            req.Function = "/" + workspaceID + "/entities";
            req.OnResponse = OnListEntitiesResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class ListEntitiesRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<ListEntitiesResp> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnListEntitiesResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            ListEntitiesResp listEntitiesResp = new ListEntitiesResp();
            fsData data = null;
            Dictionary<string, object> customData = ((ListEntitiesRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = listEntitiesResp;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "ListEntities Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((ListEntitiesRequest)req).SuccessCallback != null)
                    ((ListEntitiesRequest)req).SuccessCallback(listEntitiesResp, customData);
            }
            else
            {
                if (((ListEntitiesRequest)req).FailCallback != null)
                    ((ListEntitiesRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Create Intent

        public bool CreateIntent(SuccessCallback<Intent> successCallback, FailCallback failCallback, string workspaceID, string intent, string description = default(string), string[] examples = null, Dictionary<string, object> customData = null)
        {
            CreateIntent createIntent = new CreateIntent();
            createIntent.intent = intent;
            createIntent.description = string.IsNullOrEmpty(description) ? intent : description;
            if (examples != null)
            {
                CreateExample[] createExamples = new CreateExample[examples.Length];
                for (int i = 0; i < examples.Length; i++)
                {
                    createExamples[i] = new CreateExample() { text = examples[i] };
                }
                createIntent.examples = createExamples;
            }

            fsData data;
            _serializer.TrySerialize(createIntent.GetType(), createIntent, out data).AssertSuccessWithoutWarnings();
            string createIntentJson = fsJsonPrinter.CompressedJson(data);

            Debug.Log(createIntentJson);

            CreateIntentRequest req = new CreateIntentRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/intents";
            req.Send = Encoding.UTF8.GetBytes(createIntentJson);
            req.OnResponse = OnCreateIntentResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;
            
            return connector.Send(req);
        }

        private class CreateIntentRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Intent> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }

            // TODO: do we need this?
            public CreateIntent CreateIntent { get; set; }
        }

        private void OnCreateIntentResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Intent intent = new Intent();
            fsData data = null;
            Dictionary<string, object> customData = ((CreateIntentRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = intent;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "CreateIntent Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((CreateIntentRequest)req).SuccessCallback != null)
                    ((CreateIntentRequest)req).SuccessCallback(intent, customData);
            }
            else
            {
                if (((CreateIntentRequest)req).FailCallback != null)
                    ((CreateIntentRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Update Intent

        public bool UpdateIntent(SuccessCallback<Intent> successCallback, FailCallback failCallback, string workspaceID, string intent, string description = default(string), string[] examples = null, Dictionary<string, object> customData = null)
        {
            UpdateIntent updateIntent = new UpdateIntent();
            updateIntent.intent = intent;
            updateIntent.description = string.IsNullOrEmpty(description) ? intent : description;
            if (examples != null)
            {
                CreateExample[] createExamples = new CreateExample[examples.Length];
                for (int i = 0; i < examples.Length; i++)
                {
                    createExamples[i] = new CreateExample() { text = examples[i] };
                }
                updateIntent.examples = createExamples;
            }

            fsData data;
            _serializer.TrySerialize(updateIntent.GetType(), updateIntent, out data).AssertSuccessWithoutWarnings();
            string updateIntentJson = fsJsonPrinter.CompressedJson(data);

            Debug.Log(updateIntentJson);

            UpdateIntentRequest req = new UpdateIntentRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/intents/" + intent;
            req.Send = Encoding.UTF8.GetBytes(updateIntentJson);
            req.OnResponse = OnUpdateIntentResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;
            
            return connector.Send(req);
        }

        private class UpdateIntentRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Intent> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }

            // TODO: do we need this?
            public UpdateIntent UpdateIntent { get; set; }
        }

        private void OnUpdateIntentResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Intent intent = new Intent();
            fsData data = null;
            Dictionary<string, object> customData = ((UpdateIntentRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = intent;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "UpdateIntent Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((UpdateIntentRequest)req).SuccessCallback != null)
                    ((UpdateIntentRequest)req).SuccessCallback(intent, customData);
            }
            else
            {
                if (((UpdateIntentRequest)req).FailCallback != null)
                    ((UpdateIntentRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Get Intent

        public bool GetIntent(SuccessCallback<Intent> successCallback, FailCallback failCallback, string workspaceID, string intent, Dictionary<string, object> customData = null)
        {
            GetIntentRequest req = new GetIntentRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/intents/" + intent;
            req.OnResponse = OnGetIntentResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class GetIntentRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Intent> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }

            // TODO: do we need this?
            public String IntentName { get; set; }
        }

        private void OnGetIntentResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Intent intent = new Intent();
            fsData data = null;
            Dictionary<string, object> customData = ((GetIntentRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = intent;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "GetIntent Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((GetIntentRequest)req).SuccessCallback != null)
                    ((GetIntentRequest)req).SuccessCallback(intent, customData);
            }
            else
            {
                if (((GetIntentRequest)req).FailCallback != null)
                    ((GetIntentRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Delete Intent

        public bool DeleteIntent(SuccessCallback<bool> successCallback, FailCallback failCallback, string workspaceID, string intent, Dictionary<string, object> customData = null)
        {
            DeleteIntentRequest req = new DeleteIntentRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/intents/" + intent;
            req.Delete = true;
            req.OnResponse = OnDeleteIntentResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class DeleteIntentRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<bool> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnDeleteIntentResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Dictionary<string, object> customData = ((DeleteIntentRequest)req).CustomData;

            if (resp.Success)
            {
                if (((DeleteIntentRequest)req).SuccessCallback != null)
                    ((DeleteIntentRequest)req).SuccessCallback(resp.Success, customData);
            }
            else
            {
                if (((DeleteIntentRequest)req).FailCallback != null)
                    ((DeleteIntentRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Create Entity

        public bool CreateEntity(SuccessCallback<Entity> successCallback, FailCallback failCallback, string workspaceID, string entity, string description = default(string), CreateValue[] values = null, Dictionary<string, object> customData = null)
        {
            CreateEntity createEntity = new CreateEntity();
            createEntity.entity = entity;
            createEntity.description = string.IsNullOrEmpty(description) ? entity : description;
            createEntity.values = values;

            fsData data;
            _serializer.TrySerialize(createEntity.GetType(), createEntity, out data).AssertSuccessWithoutWarnings();
            string createEntityJson = fsJsonPrinter.CompressedJson(data);

            Debug.Log(createEntityJson);

            CreateEntityRequest req = new CreateEntityRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/entities";
            req.Send = Encoding.UTF8.GetBytes(createEntityJson);
            req.OnResponse = OnCreateEntityResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class CreateEntityRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Entity> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnCreateEntityResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Entity entity = new Entity();
            fsData data = null;
            Dictionary<string, object> customData = ((CreateEntityRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = entity;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "CreateEntity Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((CreateEntityRequest)req).SuccessCallback != null)
                    ((CreateEntityRequest)req).SuccessCallback(entity, customData);
            }
            else
            {
                if (((CreateEntityRequest)req).FailCallback != null)
                    ((CreateEntityRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Update Entity

        public bool UpdateEntity(SuccessCallback<Entity> successCallback, FailCallback failCallback, string workspaceID, string entity, string description = default(string), CreateValue[] values = null, Dictionary<string, object> customData = null)
        {
            UpdateEntity updateEntity = new UpdateEntity();
            updateEntity.entity = entity;
            updateEntity.description = string.IsNullOrEmpty(description) ? entity : description;
            updateEntity.values = values;

            fsData data;
            _serializer.TrySerialize(updateEntity.GetType(), updateEntity, out data).AssertSuccessWithoutWarnings();
            string updateEntityJson = fsJsonPrinter.CompressedJson(data);

            Debug.Log(updateEntityJson);

            UpdateEntityRequest req = new UpdateEntityRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/entities/" + entity;
            req.Send = Encoding.UTF8.GetBytes(updateEntityJson);
            req.OnResponse = OnUpdateEntityResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class UpdateEntityRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Entity> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnUpdateEntityResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Entity entity = new Entity();
            fsData data = null;
            Dictionary<string, object> customData = ((UpdateEntityRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = entity;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "UpdateEntity Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((UpdateEntityRequest)req).SuccessCallback != null)
                    ((UpdateEntityRequest)req).SuccessCallback(entity, customData);
            }
            else
            {
                if (((UpdateEntityRequest)req).FailCallback != null)
                    ((UpdateEntityRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Get Entity

        public bool GetEntity(SuccessCallback<Entity> successCallback, FailCallback failCallback, string workspaceID, string entity, Dictionary<string, object> customData = null)
        {
            GetEntityRequest req = new GetEntityRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/entities/" + entity;
            req.OnResponse = OnGetEntityResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class GetEntityRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<Entity> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnGetEntityResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Entity entity = new Entity();
            fsData data = null;
            Dictionary<string, object> customData = ((GetEntityRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = entity;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "GetEntity Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((GetEntityRequest)req).SuccessCallback != null)
                    ((GetEntityRequest)req).SuccessCallback(entity, customData);
            }
            else
            {
                if (((GetEntityRequest)req).FailCallback != null)
                    ((GetEntityRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Delete Entity

        public bool DeleteEntity(SuccessCallback<bool> successCallback, FailCallback failCallback, string workspaceID, string entity, Dictionary<string, object> customData = null)
        {
            DeleteEntityRequest req = new DeleteEntityRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/entities/" + entity;
            req.Delete = true;
            req.OnResponse = OnDeleteEntityResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class DeleteEntityRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<bool> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnDeleteEntityResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            Dictionary<string, object> customData = ((DeleteEntityRequest)req).CustomData;

            if (resp.Success)
            {
                if (((DeleteEntityRequest)req).SuccessCallback != null)
                    ((DeleteEntityRequest)req).SuccessCallback(resp.Success, customData);
            }
            else
            {
                if (((DeleteEntityRequest)req).FailCallback != null)
                    ((DeleteEntityRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion

        #region Add Entity Value

        public bool AddEntityValue(SuccessCallback<EntityValue> successCallback, FailCallback failCallback, string workspaceID, string entity, string value, string[] synonyms = null, Dictionary<string, object> customData = null)
        {
            CreateValue createValue = new CreateValue();
            createValue.value = value;
            createValue.synonyms = synonyms;

            fsData data;
            _serializer.TrySerialize(createValue.GetType(), createValue, out data).AssertSuccessWithoutWarnings();
            string createValueJson = fsJsonPrinter.CompressedJson(data);

            Debug.Log(createValueJson);

            AddEntityValueRequest req = new AddEntityValueRequest();
            req.SuccessCallback = successCallback;
            req.FailCallback = failCallback;
            req.CustomData = customData == null ? new Dictionary<string, object>() : customData;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            req.Parameters["version"] = VersionDate;
            req.Function = "/" + workspaceID + "/entities/" + entity + "/values";
            req.Send = Encoding.UTF8.GetBytes(createValueJson);
            req.OnResponse = OnAddEntityValueResp;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, "/v1/workspaces");
            if (connector == null)
                return false;

            return connector.Send(req);
        }

        private class AddEntityValueRequest : RESTConnector.Request
        {
            /// <summary>
            /// The success callback.
            /// </summary>
            public SuccessCallback<EntityValue> SuccessCallback { get; set; }
            /// <summary>
            /// The fail callback.
            /// </summary>
            public FailCallback FailCallback { get; set; }
            /// <summary>
            /// Custom data.
            /// </summary>
            public Dictionary<string, object> CustomData { get; set; }
        }

        private void OnAddEntityValueResp(RESTConnector.Request req, RESTConnector.Response resp)
        {
            EntityValue entityValue = new EntityValue();
            fsData data = null;
            Dictionary<string, object> customData = ((AddEntityValueRequest)req).CustomData;

            if (resp.Success)
            {
                try
                {
                    fsResult r = fsJsonParser.Parse(Encoding.UTF8.GetString(resp.Data), out data);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    object obj = entityValue;
                    r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
                    if (!r.Succeeded)
                        throw new WatsonException(r.FormattedMessages);

                    customData.Add("json", data);
                }
                catch (Exception e)
                {
                    Log.Error("Conversation", "AddEntityValue Exception: {0}", e.ToString());
                    resp.Success = false;
                }
            }

            if (resp.Success)
            {
                if (((AddEntityValueRequest)req).SuccessCallback != null)
                    ((AddEntityValueRequest)req).SuccessCallback(entityValue, customData);
            }
            else
            {
                if (((AddEntityValueRequest)req).FailCallback != null)
                    ((AddEntityValueRequest)req).FailCallback(resp.Error, customData);
            }
        }

        #endregion
    }
}
