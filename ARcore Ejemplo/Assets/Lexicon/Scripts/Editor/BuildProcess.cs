// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;

namespace Mixspace.Lexicon
{
#if UNITY_2018_1_OR_NEWER
    public class BuildProcess : IPreprocessBuildWithReport
#else
    public class BuildProcess : IPreprocessBuild
#endif
    {
        public int callbackOrder { get { return 0; } }

#if UNITY_2018_1_OR_NEWER
        public void OnPreprocessBuild(BuildReport report)
#else
        public void OnPreprocessBuild(BuildTarget target, string path)
#endif
        {
            LexiconConfig config = ScriptableObject.CreateInstance<LexiconConfig>();
            config.WatsonSpeechToTextUsername = PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextUsername);
            config.WatsonSpeechToTextPassword = PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonSpeechToTextPassword);
            config.WatsonConversationUsername = PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonConversationUsername);
            config.WatsonConversationPassword = PlayerPrefs.GetString(LexiconConstants.PlayerPrefs.WatsonConversationPassword);

            string tempFolder = LexiconConstants.Files.TempFolder;
            string configFile = LexiconConstants.Files.ConfigFile;

            if (!AssetDatabase.IsValidFolder("Assets/" + tempFolder))
            {
                AssetDatabase.CreateFolder("Assets", tempFolder);
            }
            if (!AssetDatabase.IsValidFolder("Assets/" + tempFolder + "/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/" + tempFolder, "Resources");
            }

            AssetDatabase.CreateAsset(config, "Assets/" + tempFolder + "/Resources/" + configFile + ".asset");

            EditorApplication.update += CheckBuildStatus;
        }

        // OnPostprocessBuild isn't triggered on build error, so we poll BuildPipeline.isBuildingPlayer instead.
        private void CheckBuildStatus()
        {
            if (!BuildPipeline.isBuildingPlayer)
            {
                EditorApplication.update -= CheckBuildStatus;
                Cleanup();
            }
        }

        private void Cleanup()
        {
            string tempFolder = LexiconConstants.Files.TempFolder;
            string configFile = LexiconConstants.Files.ConfigFile;

            AssetDatabase.DeleteAsset("Assets/" + tempFolder + "/Resources/" + configFile + ".asset");

            FileUtil.DeleteFileOrDirectory("Assets/" + tempFolder);

            AssetDatabase.Refresh();
        }
    }
}
