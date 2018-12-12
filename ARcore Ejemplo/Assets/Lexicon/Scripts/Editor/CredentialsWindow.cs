// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class CredentialsWindow : EditorWindow
    {
        private string watsonSpeechToTextUsername;
        private string watsonSpeechToTextPassword;

        private string watsonConversationUsername;
        private string watsonConversationPassword;

        [MenuItem("Lexicon/Credentials", false, 2)]
        static void Init()
        {
            CredentialsWindow window = (CredentialsWindow) GetWindow(typeof(CredentialsWindow), true, "Lexicon Credentials");
            window.Show();
        }

        void OnFocus()
        {
            watsonSpeechToTextUsername = StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextUsername));
            watsonSpeechToTextPassword = StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextPassword));

            watsonConversationUsername = StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonConversationUsername));
            watsonConversationPassword = StringUtility.Base64Decode(PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonConversationPassword));
        }

        private void OnLostFocus()
        {
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextUsername, StringUtility.Base64Encode(watsonSpeechToTextUsername));
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextPassword, StringUtility.Base64Encode(watsonSpeechToTextPassword));

            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonConversationUsername, StringUtility.Base64Encode(watsonConversationUsername));
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonConversationPassword, StringUtility.Base64Encode(watsonConversationPassword));

            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextUsername, StringUtility.Base64Encode(watsonSpeechToTextUsername));
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextPassword, StringUtility.Base64Encode(watsonSpeechToTextPassword));

            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonConversationUsername, StringUtility.Base64Encode(watsonConversationUsername));
            PlayerPrefs.SetString(LexiconConstants.PlayerPrefs.WatsonConversationPassword, StringUtility.Base64Encode(watsonConversationPassword));

            PlayerPrefs.Save();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Changing the Company Name or Product Name for this project will reset these credentials.", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("These credentials will not be committed to version control. They will be included as a resource in a build. To learn more about securing your credentials see the documentation.", MessageType.Info);

            EditorGUILayout.Space();

            GUILayout.Label("Watson Speech To Text", EditorStyles.boldLabel);

            watsonSpeechToTextUsername = EditorGUILayout.TextField("Username", watsonSpeechToTextUsername);
            watsonSpeechToTextPassword = EditorGUILayout.TextField("Password", watsonSpeechToTextPassword);

            EditorGUILayout.Space();

            GUILayout.Label("Watson Conversation", EditorStyles.boldLabel);

            watsonConversationUsername = EditorGUILayout.TextField("Username", watsonConversationUsername);
            watsonConversationPassword = EditorGUILayout.TextField("Password", watsonConversationPassword);
        }
    }
}
