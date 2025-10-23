using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
namespace TrungKien.Core
{
    public class EvenlySpacedCurvePoints3D : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] bool isTransform = false;
        [HideIf(nameof(isTransform))] public Vector3 startPos = new Vector3(-5, 0, 0), endPos = new Vector3(5, 0, 0);
        [ShowIf(nameof(isTransform))] public Transform tranStartPoint, tranEndPoint;

        [Header("Curve Settings")]
        [Range(0f, 1f)] public float curveHeight = 0.5f; // độ cong (tỉ lệ theo khoảng cách)
        [Range(3, 50)] public int pointCount = 10;       // số lượng điểm
        public enum CurveAxis { Y_Up, Z_Forward, X_Right, Custom }
        public CurveAxis curveAxis = CurveAxis.Y_Up;
        bool isCustomDirection => curveAxis == CurveAxis.Custom;
        [ShowIf(nameof(isCustomDirection))] public Vector3 customDirection = Vector3.up; // hướng cong tùy chỉnh

        [Header("Gizmos")]
        public bool showGizmos = true;
        [ShowIf(nameof(showGizmos))] public Color gizmoColor = Color.yellow;

        public List<Vector3> evenlySpacedPoints = new List<Vector3>();

        void OnValidate()
        {
            GenerateEvenlySpacedCurve();
        }

        void GenerateEvenlySpacedCurve()
        {
            evenlySpacedPoints.Clear();

            // --- Tính vector hướng cong ---
            Vector3 axisDir = Vector3.up;
            switch (curveAxis)
            {
                case CurveAxis.Z_Forward:
                    axisDir = Vector3.forward;
                    break;
                case CurveAxis.Custom:
                    axisDir = customDirection;
                    break;
                case CurveAxis.Y_Up:
                    axisDir = Vector3.up;
                    break;
                case CurveAxis.X_Right:
                    axisDir = Vector3.right;
                    break;
            }
            evenlySpacedPoints = Extension.GetPointCurve(axisDir, isTransform ? tranStartPoint.position : startPos, isTransform ? tranEndPoint.position : endPos, curveHeight, pointCount);
        }

        void OnDrawGizmos()
        {
            if (!showGizmos || evenlySpacedPoints.Count < 2) return;
            Gizmos.color = gizmoColor;
            for (int i = 0; i < evenlySpacedPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(evenlySpacedPoints[i], evenlySpacedPoints[i + 1]);
                Gizmos.DrawSphere(evenlySpacedPoints[i], 0.08f);
            }
            Gizmos.DrawSphere(evenlySpacedPoints[evenlySpacedPoints.Count - 1], 0.08f);
        }
    }
}