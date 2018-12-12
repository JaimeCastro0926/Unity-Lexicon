// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class UpdateActionPrefab : SyncAction
    {
        public LexiconIntent intent;

        public static UpdateActionPrefab CreateInstance(LexiconIntent intent)
        {
            UpdateActionPrefab instance = CreateInstance<UpdateActionPrefab>();
            instance.intent = intent;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (intent == null)
            {
                // Intent asset was deleted
                succeeded = false;
                isDone = true;
                return;
            }

            string normalizedName = StringUtility.ToCamelCase(intent.ActionName);

            string path = AssetDatabase.GetAssetPath(intent);
            string directory = Path.GetDirectoryName(path);
            string filename = normalizedName + "Action";
            string prefabPath = directory + "/" + filename + ".prefab";

            string typeName = intent.NamespaceString + "." + filename;
            Type actionType = Type.GetType(typeName + ",Assembly-CSharp");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.Log("Could not find prefab: " + prefabPath);
                succeeded = false;
                isDone = true;
            }
            else if (actionType == null)
            {
                Debug.Log("Could not find type: " + typeName);
                succeeded = false;
                isDone = true;
            }
            else
            {
                LexiconAction newAction = (LexiconAction)prefab.AddComponent(actionType);

                LexiconPlaceholderAction tempAction = prefab.GetComponent<LexiconPlaceholderAction>();
                if (intent.DefaultAction == tempAction)
                {
                    Debug.Log("Replacing temp action");
                    intent.DefaultAction = newAction;
                    GameObject.DestroyImmediate(tempAction, true);
                }

                // TODO: Set dirty?

                succeeded = true;
                isDone = true;
            }
        }
    }
}

#endif
