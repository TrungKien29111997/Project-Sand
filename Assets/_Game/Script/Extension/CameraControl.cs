using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TrungKien
{
    public class CameraControl : PoolingElement
    {
        public Camera camera;
        public float valueSizeCamera { get; private set; }
        public float defaultSizeCamera { get; private set; }
        [SerializeField]
        float
    topClamp = 180f,
    bottomClamp = -180f,
    sensitivity = 2f;
        float cinemachineTargetYaw;
        float cinemachineTargetPitch;
        private const float threshold = 0.01f;
        void Start()
        {
            defaultSizeCamera = camera.fieldOfView;
        }
        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                CameraRotation();
            }
        }
        float mouseX, mouseY;
        void CameraRotation()
        {
            mouseX = Input.GetAxis("Mouse X") * sensitivity;
            mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            // if there is an input and camera position is not fixed
            if ((Mathf.Abs(mouseX) > threshold || Mathf.Abs(mouseY) > threshold))
            {
                cinemachineTargetYaw += mouseX;
                cinemachineTargetPitch -= mouseY; // trừ để trục Y chuột đúng chiều xoay camera
            }

            // clamp our rotations so our values are limited 360 degrees
            cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

            // Cinemachine will follow this target
            TF.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);

        }
        float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}