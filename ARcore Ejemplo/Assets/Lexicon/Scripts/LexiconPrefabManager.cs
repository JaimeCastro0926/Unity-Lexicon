// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class LexiconPrefabManager : MonoBehaviour
    {
        public static LexiconPrefabManager Instance { get { return Singleton<LexiconPrefabManager>.Instance; } }

        private Dictionary<GameObject, GameObject> instanceObjects = new Dictionary<GameObject, GameObject>();

        public void OnEnable()
        {
            Instance.hideFlags = HideFlags.None;
        }

        public bool Exists(GameObject prefab)
        {
            GameObject instance;

            instanceObjects.TryGetValue(prefab, out instance);

            return (instance != null);
        }

        public GameObject FindOrCreate(GameObject prefab)
        {
            GameObject instance;

            instanceObjects.TryGetValue(prefab, out instance);

            if (instance == null)
            {
                Debug.Log("Creating instance of prefab: " + prefab);
                instance = GameObject.Instantiate(prefab);
                instanceObjects[prefab] = instance;
            }

            return instance;
        }

        public void DestroyInstance(GameObject prefab)
        {
            GameObject instance;

            if (instanceObjects.TryGetValue(prefab, out instance))
            {
                Debug.Log("Destroying instance of prefab: " + prefab);
                instanceObjects.Remove(prefab);
                Destroy(instance);
            }
        }
    }
}
