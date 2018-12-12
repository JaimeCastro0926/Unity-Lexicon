// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using Mixspace.Lexicon;
using UnityEngine;

namespace Mixspace.Lexicon.Samples
{
    public class GazeFocus : MonoBehaviour
    {
        public Material pointerMaterial;

        public float pointerSize = 0.01f;

        public float dwellSpeed = 0.1f;

        private LexiconFocusManager focusManager;

        private Camera mainCamera;

        private GameObject pointerSphere;

        private Vector3 lastPosition;

        void OnEnable()
        {
            // Register for the capture focus callback.
            LexiconFocusManager.OnCaptureFocus += CaptureFocus;
        }

        void OnDisable()
        {
            LexiconFocusManager.OnCaptureFocus -= CaptureFocus;
        }

        void Start()
        {
            focusManager = LexiconFocusManager.Instance;
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("GazeFocus requires a camera tagged Main Camera");
                enabled = false;
            }

            // This script adds a simple pointer.
            // If you are using an XR library that provides a pointer you won't need this.
            pointerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointerSphere.GetComponent<Renderer>().material = pointerMaterial;
            pointerSphere.transform.localScale = new Vector3(pointerSize, pointerSize, pointerSize);
            Destroy(pointerSphere.GetComponent<Collider>());
        }

        public void CaptureFocus()
        {
            // Get a focus position data entry from the pool.
            FocusPosition focusPosition = focusManager.GetPooledData<FocusPosition>();

            Ray cameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(cameraRay, out hit))
            {
                // This sample uses a LexiconSelectable component to determine object selectability.
                // You could just as easily use layers, tags, or your own scripts.
                LexiconSelectable selectable = hit.collider.gameObject.GetComponentInParent<LexiconSelectable>();
                if (selectable != null)
                {
                    FocusSelection focusSelection = focusManager.GetPooledData<FocusSelection>();
                    focusSelection.SelectedObject = selectable.gameObject;
                    focusManager.AddFocusData(focusSelection);
                }

                // Set the focus position to the hit point if present.
                focusPosition.Position = hit.point;
                focusPosition.Normal = hit.normal;
            }
            else
            {
                // Set the focus position in front of the camera if no hit point.
                focusPosition.Position = mainCamera.transform.position + mainCamera.transform.forward * 2.0f;
                focusPosition.Normal = -mainCamera.transform.forward;
            }

            // Add our focus position data entry.
            focusManager.AddFocusData(focusPosition);

            // Update the pointer position.
            pointerSphere.transform.position = focusPosition.Position;
            float dist = Vector3.Distance(focusPosition.Position, mainCamera.transform.position);
            float scale = pointerSize * dist;
            pointerSphere.transform.localScale = new Vector3(scale, scale, scale);

            // Add a dwell position entry if the user's gaze is lingering on a spot.
            float speed = Vector3.Magnitude(lastPosition - focusPosition.Position) / Time.deltaTime;
            if (speed < dwellSpeed)
            {
                FocusDwellPosition dwellPosition = focusManager.GetPooledData<FocusDwellPosition>();
                dwellPosition.Position = focusPosition.Position;
                dwellPosition.Normal = focusPosition.Normal;
                focusManager.AddFocusData(dwellPosition);
            }

            lastPosition = focusPosition.Position;
        }
    }
}
