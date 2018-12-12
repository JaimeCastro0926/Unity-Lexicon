// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class SyncAction : ScriptableObject
    {
        [NonSerialized]
        public bool isDone;

        [NonSerialized]
        public bool succeeded;

        [NonSerialized]
        public bool retry;

        [NonSerialized]
        public float retryDelay;

        public void OnEnable()
        {
            //Debug.Log("SyncAction " + this.GetType() + " OnEnable");
            hideFlags = HideFlags.HideAndDontSave;
        }

        public void OnDestroy()
        {
            //Debug.Log("SyncAction " + this.GetType() + " OnDestroy");
        }

        public virtual void Process(SyncQueue queue)
        {
            Debug.LogWarning("SyncAction base class Process called");
        }
    }
}

#endif
