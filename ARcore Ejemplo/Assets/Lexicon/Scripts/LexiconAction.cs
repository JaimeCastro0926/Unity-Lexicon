// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

namespace Mixspace.Lexicon
{
    public class LexiconAction : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Whether to destroy this GameObject on Process complete.")]
        private bool destroyOnComplete = true;

        [SerializeField]
        [Tooltip("Whether to create a single reusable instance of this GameObject.")]
        private bool singleton = true;

        /// <summary>
        /// Whether to destroy this GameObject on Process complete.
        /// </summary>
        public bool DestroyOnComplete
        {
            get { return destroyOnComplete; }
            set { destroyOnComplete = value; }
        }

        /// <summary>
        /// Whether to create a single reusable instance of this GameObject.
        /// </summary>
        public bool Singleton
        {
            get { return singleton; }
            set { singleton = value; }
        }

        public delegate void DestroyHandler();
        public event DestroyHandler OnDestroyAction;

        public void OnDestroy()
        {
            if (OnDestroyAction != null)
            {
                OnDestroyAction();
            }
        }

        /// <summary>
        /// Override this method to handle this intent when matched at runtime.
        /// Return true if this event was consumed, false to allow other intents to handle.
        /// </summary>
        public virtual bool Process(LexiconRuntimeResult runtimeResult)
        {
            return false;
        }
    }
}
