// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using UnityEngine;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace Mixspace.Lexicon.Samples
{
    // Simple keyboard controller with mouse look.
    // Will be disabled if XR is enabled in player settings.
    public class SimpleFPSController : MonoBehaviour
    {
        public float lookSpeed = 1.0f;
        public float moveSpeed = 0.1f;

        private GameObject controller;

        private float xRotation = 0.0f;

        private void OnEnable()
        {
#if UNITY_2017_2_OR_NEWER
            if (XRSettings.enabled)
            {
                enabled = false;
            }
#else
            if (VRSettings.enabled)
            {
                enabled = false;
            }
#endif
        }

        void Start()
        {
            controller = new GameObject("Controller");
            controller.transform.position = gameObject.transform.position;
            controller.transform.rotation = gameObject.transform.rotation;
            gameObject.transform.parent = controller.transform;
            gameObject.transform.localPosition = new Vector3(0, 0, 0);

            Cursor.visible = false;
        }

        void Update()
        {
            float xRot = Input.GetAxis("Mouse Y") * lookSpeed;
            float yRot = Input.GetAxis("Mouse X") * lookSpeed;

            controller.transform.rotation *= Quaternion.Euler(0.0f, yRot, 0.0f);

            xRotation -= xRot;
            xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);
            transform.localEulerAngles = new Vector3(xRotation, 0.0f, 0.0f);

            float forwardMove = Input.GetAxis("Vertical") * moveSpeed;
            float rightMove = Input.GetAxis("Horizontal") * moveSpeed;

            controller.transform.position += controller.transform.forward * forwardMove;
            controller.transform.position += controller.transform.right * rightMove;
        }
    }
}
