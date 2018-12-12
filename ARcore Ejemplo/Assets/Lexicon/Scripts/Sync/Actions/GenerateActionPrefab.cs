// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class GenerateActionPrefab : SyncAction
    {
        public LexiconIntent intent;

        public static GenerateActionPrefab CreateInstance(LexiconIntent intent)
        {
            GenerateActionPrefab instance = CreateInstance<GenerateActionPrefab>();
            instance.intent = intent;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            string normalizedName = StringUtility.ToCamelCase(intent.ActionName);

            string path = AssetDatabase.GetAssetPath(intent);
            string directory = Path.GetDirectoryName(path);
            string filename = normalizedName + "Action";
            string prefabPath = directory + "/" + filename + ".prefab";

            GameObject prefabTemplate = new GameObject(filename);
            prefabTemplate.AddComponent<LexiconPlaceholderAction>();

            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, prefabTemplate);
            intent.DefaultAction = prefab.GetComponent<LexiconAction>();

            GameObject.DestroyImmediate(prefabTemplate);

            succeeded = true;
            isDone = true;
        }
    }
}

#endif
