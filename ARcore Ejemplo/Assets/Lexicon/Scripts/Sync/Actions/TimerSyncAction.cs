// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class TimerSyncAction : SyncAction
    {
        public float duration;

        private double startTime;

        public static TimerSyncAction CreateInstance(float seconds)
        {
            TimerSyncAction instance = CreateInstance<TimerSyncAction>();
            instance.duration = seconds;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += Tick;
        }

        public void Tick()
        {
            double dt = EditorApplication.timeSinceStartup - startTime;

            if (dt > duration)
            {
                EditorApplication.update -= Tick;
                succeeded = true;
                isDone = true;
            }
        }
    }
}

#endif
