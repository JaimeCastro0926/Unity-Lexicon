// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Utilities;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class SyncQueue : ScriptableObject
    {
        [SerializeField]
        protected List<SyncAction> queue = new List<SyncAction>();

        [SerializeField]
        protected SyncAction onCompleteAction;

        [SerializeField]
        protected SyncAction currentAction;

        [NonSerialized]
        protected bool processing;
        [NonSerialized]
        protected int processId;
        [NonSerialized]
        protected bool dirty;
        [NonSerialized]
        protected bool finished;

        public delegate void QueueFinishedHandler();
        public event QueueFinishedHandler OnQueueFinished;

        public void OnEnable()
        {
            //Debug.Log("SyncQueue OnEnable");
            hideFlags = HideFlags.HideAndDontSave;
        }

        public void OnDestroy()
        {
            //Debug.Log("SyncQueue OnDestroy");
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(queue[i]);
            }
        }

        public SyncAction OnCompleteAction
        {
            set { onCompleteAction = value; }
        }

        public bool IsFinished
        {
            get { return finished; }
        }

        public void Enqueue(SyncAction action)
        {
            queue.Add(action);
        }

        public SyncAction Peek()
        {
            if (queue.Count > 0)
            {
                return queue[0];
            }

            return null;
        }

        public SyncAction Dequeue()
        {
            SyncAction action = null;

            if (queue.Count > 0)
            {
                action = queue[0];
                queue.RemoveAt(0);
            }

            currentAction = action;
            return action;
        }

        public void Requeue(SyncAction action)
        {
            queue.Insert(0, action);
        }

        public void RequeueCurrentAction()
        {
            if (currentAction != null)
            {
                Requeue(currentAction);
                currentAction = null;
            }
        }

        public void Insert(SyncAction action, int index)
        {
            queue.Insert(index, action);
        }

        public void Process()
        {
            if (!processing)
            {
                //Debug.Log("Process, currentAction: " + currentAction);

                if (currentAction != null)
                {
                    // Action was interrupted before complete (e.g. due to an assembly reload).
                    RequeueCurrentAction();
                }

                processId = Runnable.Run(ProcessQueue());
            }
        }

        public void Cancel()
        {
            Debug.Log("Cancelling Queue " + processId);

            Runnable.Stop(processId);
            finished = true;
        }

        private IEnumerator ProcessQueue()
        {
            if (finished)
            {
                // Already finished, exit the coroutine.
                yield break;
            }

            processing = true;
            bool failed = false;

            SyncAction action = Dequeue();

            while (action != null)
            {
                Debug.Log("Processing Action: " + action.GetType().Name);

                yield return null;

                // Reset all fields in case this is a retry.
                action.isDone = false;
                action.succeeded = false;
                action.retry = false;
                action.retryDelay = 0.0f;

                action.Process(this);

                while (!action.isDone)
                {
                    yield return null;
                }

                Debug.Log("Action " + action.GetType().Name + " " + (action.succeeded ? "Succeeded" : (action.retry ? "Retry" : "Failed")));

                if (!action.succeeded)
                {
                    RequeueCurrentAction();

                    if (!action.retry)
                    {
                        // Fatal error, stop the queue.
                        failed = true;
                        DestroyImmediate(action);
                        break;
                    }
                    else if (action.retryDelay > 0)
                    {
                        // Wait delay seconds and try again.
                        Insert(TimerSyncAction.CreateInstance(action.retryDelay), 0);
                    }
                }
                else
                {
                    DestroyImmediate(action);
                }

                yield return null;

                action = Dequeue();
            }

            if (onCompleteAction != null)
            {
                onCompleteAction.succeeded = !failed;
                onCompleteAction.Process(this);
                DestroyImmediate(onCompleteAction);
            }

            processing = false;
            finished = true;

            if (OnQueueFinished != null)
            {
                OnQueueFinished();
            }
        }
    }
}

#endif
