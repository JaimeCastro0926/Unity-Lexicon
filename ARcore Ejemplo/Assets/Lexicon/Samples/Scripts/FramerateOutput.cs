// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Mixspace.Lexicon.Samples
{
    public class FramerateOutput : MonoBehaviour
    {
        public float updateInterval = 0.5f;
        public Text outputText;

        private float accum;
        private int frames;
        private float timeleft;

        void Start()
        {
            timeleft = updateInterval;
        }

        void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;

            if (timeleft <= 0.0)
            {
                float fps = accum / frames;
                string format = System.String.Format("{0:F0} FPS", fps);
                outputText.text = format;

                timeleft = updateInterval;
                accum = 0.0f;
                frames = 0;
            }
        }
    }
}
