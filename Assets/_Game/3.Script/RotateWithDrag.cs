using Sirenix.OdinInspector;
using UnityEngine;

namespace TrungKien
{
    public class RotateWithDrag : PoolingElement
    {
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float rotationRate = 3.0f;
        [SerializeField] private bool xRotation;
        [SerializeField] private bool yRotation;
        [ShowIf(nameof(yRotation))][SerializeField] private Vector2 minMaxX;
        [SerializeField] private bool invertX;
        [SerializeField] private bool invertY;
        [SerializeField] private bool touchAnywhere;
        private float m_previousX;
        private float m_previousY;
        private Camera m_camera;
        private bool m_rotating = false;
        float cinemachineTargetYaw;
        float cinemachineTargetPitch;

        private void Awake()
        {
            m_camera = Camera.main;
        }

        private void Update()
        {
            if (!touchAnywhere)
            {
                //No need to check if already rotating
                if (!m_rotating)
                {
                    RaycastHit hit;
                    Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
                    if (!Physics.Raycast(ray, out hit, 1000, targetLayer))
                    {
                        return;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                m_rotating = true;
                m_previousX = Input.mousePosition.x;
                m_previousY = Input.mousePosition.y;
            }
            // get the user touch input
            if (Input.GetMouseButton(0))
            {
                var touch = Input.mousePosition;
                var deltaX = -(touch.y - m_previousY) * rotationRate;
                var deltaY = -(touch.x - m_previousX) * rotationRate;
                if (!yRotation) deltaX = 0;
                if (!xRotation) deltaY = 0;
                if (invertX) deltaY *= -1;
                if (invertY) deltaX *= -1;

                //cinemachineTargetYaw += deltaX;
                //cinemachineTargetPitch += deltaY;

                // clamp our rotations so our values are limited 360 degrees
                //cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
                //cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

                transform.Rotate(deltaX, deltaY, 0, Space.World);
                //transform.rotation = Quaternion.Euler(cinemachineTargetPitch + CameraAngleOverride, cinemachineTargetYaw, 0.0f);

                m_previousX = Input.mousePosition.x;
                m_previousY = Input.mousePosition.y;
            }
            if (Input.GetMouseButtonUp(0))
                m_rotating = false;
        }
        float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
