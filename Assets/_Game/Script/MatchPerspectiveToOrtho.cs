using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TrungKien.Core
{
    public class MatchPerspectiveToOrtho : MonoBehaviour
    {
        public Camera camMain;   // Camera perspective
        public Camera camUI;     // Camera orthographic (overlay)
        public Canvas canvas;    // World Space Canvas (render bởi camUI)
        public float matchPlaneDistance = 10f; // khoảng cách cần khớp (tính từ camMain)

        void LateUpdate()
        {
            if (!camMain || !camUI) return;

            // Tính halfHeight tương ứng FOV tại khoảng cách này
            float halfHeight = Mathf.Tan(camMain.fieldOfView * 0.5f * Mathf.Deg2Rad) * matchPlaneDistance;
            float halfWidth = halfHeight * camMain.aspect;

            // Gán lại kích thước cho camera orthographic để match góc nhìn
            camUI.orthographicSize = halfHeight;

            // Đặt vị trí UI camera trùng hướng với main camera, nhưng ở khoảng cách matchPlaneDistance
            camUI.transform.position = camMain.transform.position + camMain.transform.forward * matchPlaneDistance;
            camUI.transform.rotation = camMain.transform.rotation;

            // Nếu có canvas World Space thì cho nó khớp luôn vị trí camera
            if (canvas)
            {
                canvas.worldCamera = camUI;
                canvas.transform.position = camUI.transform.position;
                canvas.transform.rotation = camUI.transform.rotation;
            }
        }
    }
}