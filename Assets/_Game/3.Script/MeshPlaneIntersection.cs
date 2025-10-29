using UnityEngine;
using System.Collections.Generic;

public class MeshPlaneIntersection : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Transform planeTransform;

    public List<Vector3> GetIntersectionPoints()
    {
        List<Vector3> intersections = new List<Vector3>();

        if (meshFilter == null) return intersections;

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Định nghĩa plane từ transform
        Plane plane = new Plane(planeTransform.up, planeTransform.position);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = meshFilter.transform.TransformPoint(vertices[triangles[i]]);
            Vector3 p2 = meshFilter.transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 p3 = meshFilter.transform.TransformPoint(vertices[triangles[i + 2]]);

            // khoảng cách từ điểm tới plane
            float d1 = plane.GetDistanceToPoint(p1);
            float d2 = plane.GetDistanceToPoint(p2);
            float d3 = plane.GetDistanceToPoint(p3);

            // check từng cạnh
            CheckEdge(p1, d1, p2, d2, plane, intersections);
            CheckEdge(p2, d2, p3, d3, plane, intersections);
            CheckEdge(p3, d3, p1, d1, plane, intersections);
        }

        return intersections;
    }

    private void CheckEdge(Vector3 p1, float d1, Vector3 p2, float d2, Plane plane, List<Vector3> intersections)
    {
        if ((d1 > 0f && d2 < 0f) || (d1 < 0f && d2 > 0f))
        {
            float t = d1 / (d1 - d2);
            Vector3 hit = Vector3.Lerp(p1, p2, t);
            intersections.Add(hit);
        }
    }
}
