// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class LexiconFocusManager : MonoBehaviour
    {
        private static LexiconFocusManager instance;

        [SerializeField]
        [Tooltip("How many seconds worth of records we should keep.")]
        private float bufferLength = 10.0f;

        /// <summary>
        /// Gets the focus manager instance.
        /// </summary>
        public static LexiconFocusManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LexiconFocusManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject("LexiconFocusManager");
                        instance = obj.AddComponent<LexiconFocusManager>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// How many seconds worth of records we should keep.
        /// </summary>
        public float BufferLength
        {
            get { return bufferLength; }
        }

        /// <summary>
        /// Optional callback for adding data entries.
        /// </summary>
        public delegate void CaptureFocus();
        public static event CaptureFocus OnCaptureFocus;

        /// <summary>
        /// Store the data entries by type for faster searching.
        /// </summary>
        private Dictionary<Type, List<LexiconFocusData>> focusDataDict = new Dictionary<Type, List<LexiconFocusData>>();

        /// <summary>
        /// Reuse previously created data entries to avoid allocations each frame.
        /// </summary>
        private Dictionary<Type, List<LexiconFocusData>> focusDataPool = new Dictionary<Type, List<LexiconFocusData>>();

        /// <summary>
        /// Record data entries each frame.
        /// </summary>
        void Update()
        {
            if (OnCaptureFocus != null)
            {
                OnCaptureFocus();
            }
        }

        /// <summary>
        /// Remove old entries at the end of the frame.
        /// </summary>
        private void LateUpdate()
        {
            float cutoff = Time.realtimeSinceStartup - bufferLength;

            foreach (List<LexiconFocusData> list in focusDataDict.Values)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Timestamp < cutoff)
                    {
                        if (list[i].IsPooled)
                        {
                            ReturnToPool(list[i]);
                        }
                        list.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve a data entry from the pool for a particular type. Use this to avoid allocations.
        /// </summary>
        public T GetPooledData<T>() where T : LexiconFocusData, new()
        {
            List<LexiconFocusData> dataPool;
            if (focusDataPool.TryGetValue(typeof(T), out dataPool))
            {
                if (dataPool.Count > 0)
                {
                    LexiconFocusData data = dataPool[0];
                    dataPool.RemoveAt(0);
                    data.Timestamp = Time.realtimeSinceStartup;
                    return (T)data;
                }
            }

            T newData = new T();
            newData.IsPooled = true;
            return newData;
        }

        /// <summary>
        /// Record a focus data entry.
        /// </summary>
        public void AddFocusData(LexiconFocusData data)
        {
            List<LexiconFocusData> dataEntries;
            if (focusDataDict.TryGetValue(data.GetType(), out dataEntries))
            {
                dataEntries.Add(data);
            }
            else
            {
                List<LexiconFocusData> newList = new List<LexiconFocusData>();
                newList.Add(data);
                focusDataDict.Add(data.GetType(), newList);
            }
        }

        /// <summary>
        /// Gets the closest data entry of type T to the given timestamp, within maxTimeOffset seconds.
        /// </summary>
        public T GetFocusData<T>(float realtime, float maxTimeOffset = 0.5f) where T : LexiconFocusData
        {
            float minDist = float.MaxValue;
            LexiconFocusData result = null;

            List<LexiconFocusData> dataEntries;
            if (focusDataDict.TryGetValue(typeof(T), out dataEntries))
            {
                foreach (LexiconFocusData data in dataEntries)
                {
                    // TODO: Optimize this.
                    float dist = Mathf.Abs(realtime - data.Timestamp);
                    if (dist < minDist && dist < maxTimeOffset)
                    {
                        minDist = dist;
                        result = data;
                    }
                }
            }

            return (T)result;
        }

        /// <summary>
        /// Gets the closest data entry of type T after the given timestamp, within maxTimeOffset seconds.
        /// </summary>
        public T GetFocusDataAfter<T>(float realtime, float maxTimeOffset = 0.5f) where T : LexiconFocusData
        {
            List<LexiconFocusData> dataEntries;
            if (focusDataDict.TryGetValue(typeof(T), out dataEntries))
            {
                foreach (LexiconFocusData data in dataEntries)
                {
                    if (data.Timestamp >= realtime)
                    {
                        float dist = realtime - data.Timestamp;
                        if (dist < maxTimeOffset)
                        {
                            return (T)data;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the data entry to the pool.
        /// </summary>
        private void ReturnToPool(LexiconFocusData data)
        {
            List<LexiconFocusData> dataPool;
            if (focusDataPool.TryGetValue(data.GetType(), out dataPool))
            {
                dataPool.Add(data);
            }
            else
            {
                List<LexiconFocusData> newPool = new List<LexiconFocusData>();
                newPool.Add(data);
                focusDataPool.Add(data.GetType(), newPool);
            }
        }

        /// <summary>
        /// Prints debug info.
        /// </summary>
        private void PrintDebug()
        {
            Debug.Log("Data Entries:");
            foreach (Type t in focusDataDict.Keys)
            {
                Debug.Log("  " + t + ": " + focusDataDict[t].Count);
            }

            Debug.Log("Data Pools:");
            foreach (Type t in focusDataPool.Keys)
            {
                Debug.Log("  " + t + ": " + focusDataPool[t].Count);
            }
        }
    }
}
