// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Mixspace.Lexicon.Samples
{
    // Sample script for indicating position from partial speech results.
    // Useful for giving the user early feedback.
    public class PositionDetector : MonoBehaviour
    {
        public LexiconEntity positionEntity;

        public Shader markerShader;
        public Color markerColor = Color.white;
        public float markerScale = 0.02f;

        private LexiconFocusManager focusManager;

        private List<GameObject> markers = new List<GameObject>();
        private Material markerMaterial;

        void OnEnable()
        {
            LexiconRuntime.OnSpeechToTextResults += OnSpeechToTextResult;
            LexiconRuntime.OnLexiconResults += OnLexiconResults;
        }

        void OnDisable()
        {
            LexiconRuntime.OnSpeechToTextResults -= OnSpeechToTextResult;
            LexiconRuntime.OnLexiconResults -= OnLexiconResults;
        }

        void Start()
        {
            if (positionEntity == null)
            {
                Debug.LogError("PositionDetector requires a Position entity");
                enabled = false;
            }

            focusManager = LexiconFocusManager.Instance;

            markerMaterial = new Material(markerShader);
            markerMaterial.color = markerColor;
        }

        void OnSpeechToTextResult(LexiconSpeechResult speechResult)
        {
            foreach (GameObject markerObject in markers)
            {
                Destroy(markerObject);
            }
            markers.Clear();

            foreach (LexiconSpeechResult.WordResult wordResult in speechResult.WordResults)
            {
                if (positionEntity.FindValueByName(wordResult.Word, true) != null)
                {
                    FocusPosition focusPosition = focusManager.GetFocusData<FocusPosition>(wordResult.RealtimeStart);
                    if (focusPosition != null)
                    {
                        GameObject markerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        markerObject.transform.position = focusPosition.Position;
                        markerObject.transform.localScale = new Vector3(markerScale, markerScale, markerScale);
                        markerObject.transform.parent = this.transform;
                        markerObject.GetComponent<Renderer>().material = markerMaterial;
                        markerObject.GetComponent<SphereCollider>().enabled = false;
                        markers.Add(markerObject);
                    }
                }
            }
        }

        void OnLexiconResults(List<LexiconRuntimeResult> results)
        {
            foreach (GameObject markerObject in markers)
            {
                Destroy(markerObject);
            }
            markers.Clear();
        }
    }
}
