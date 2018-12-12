// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using Mixspace.Lexicon;
using UnityEngine;

namespace Mixspace.Lexicon.Samples
{
    public class MouseFocus : MonoBehaviour
    {
        public float dwellSpeed = 0.01f;

        private LexiconFocusManager focusManager;

        private Camera mainCamera;

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
        }

        public void CaptureFocus()
        {
            // Get a focus position data entry from the pool.
            FocusPosition focusPosition = focusManager.GetPooledData<FocusPosition>();

            Ray pointerRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(pointerRay, out hit))
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
                // Set the focus position in front of the mouse if no hit point.
                focusPosition.Position = pointerRay.GetPoint(1.5f);
                focusPosition.Normal = -pointerRay.direction;
            }

            // Add our focus position data entry.
            focusManager.AddFocusData(focusPosition);

            // Add a dwell position entry if the mouse is lingering on a spot.
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
