#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class SmartMeshDeduperPro : MonoBehaviour
{
    [Header("Deduplicate Settings")]
    [Tooltip("Kiểm tra các mesh mirror (scale âm)")]
    public bool isCheckMirrorMesh = false;

    [Range(0.0001f, 0.01f)]
    public float tolerance = 0.001f;

    [Button]
    public void DeduplicateMeshes()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(true);
        var uniqueList = new List<Mesh>();
        int replaced = 0;
        int mirrored = 0;

        foreach (var mf in filters)
        {
            var mesh = mf.sharedMesh;
            if (!mesh) continue;

            bool found = false;
            foreach (var unique in uniqueList)
            {
                // ✅ Check duplicate mesh
                if (IsMeshIdentical(mesh, unique, tolerance))
                {
                    mf.sharedMesh = unique;
                    replaced++;
                    found = true;
                    break;
                }

                // ✅ Check mirror mesh (tùy chọn)
                if (isCheckMirrorMesh && TryFindMirror(mesh, unique, mf.transform))
                {
                    mf.sharedMesh = unique;
                    mirrored++;
                    found = true;
                    break;
                }
            }

            if (!found)
                uniqueList.Add(mesh);
        }

        Debug.Log($"✅ Done. {replaced} duplicates removed, {mirrored} mirrors unified. Unique meshes: {uniqueList.Count}");
    }

    // ------------------ GEOMETRY CHECKS ------------------

    bool IsMeshIdentical(Mesh a, Mesh b, float tol)
    {
        if (a.vertexCount != b.vertexCount) return false;
        var av = a.vertices;
        var bv = b.vertices;
        var aSet = new HashSet<Vector3>(av.Select(v => RoundVec(v, tol)));
        var bSet = new HashSet<Vector3>(bv.Select(v => RoundVec(v, tol)));
        return aSet.SetEquals(bSet);
    }

    bool TryFindMirror(Mesh a, Mesh b, Transform t)
    {
        if (a.vertexCount != b.vertexCount) return false;

        Vector3[] av = a.vertices;
        Vector3[] bv = b.vertices;

        // Các hướng mirror có thể
        Vector3[] mirrorNormals =
        {
            Vector3.right, Vector3.up, Vector3.forward
        };

        var bSet = new HashSet<Vector3>(bv.Select(v => RoundVec(v, tolerance)));

        foreach (var normal in mirrorNormals)
        {
            Matrix4x4 reflect = ReflectionMatrix(normal);
            var mirroredA = av.Select(v => RoundVec(reflect.MultiplyPoint3x4(v), tolerance));

            if (mirroredA.All(v => bSet.Contains(v)))
            {
                // ✅ Nếu khớp, ta flip scale theo trục phản chiếu
                FlipTransformScale(t, normal);
                return true;
            }
        }

        return false;
    }

    // ------------------ HELPERS ------------------

    static void FlipTransformScale(Transform t, Vector3 normal)
    {
        Vector3 s = t.localScale;

        // Xác định trục nào gần nhất với vector normal
        Vector3 absNormal = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));

        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
            s.x *= -1;
        else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
            s.y *= -1;
        else
            s.z *= -1;

        t.localScale = s;
        Debug.Log($"↔ Flipped scale of {t.name} → {s}");
    }

    static Matrix4x4 ReflectionMatrix(Vector3 n)
    {
        n.Normalize();
        float nx = n.x, ny = n.y, nz = n.z;
        return new Matrix4x4(
            new Vector4(1 - 2 * nx * nx, -2 * nx * ny, -2 * nx * nz, 0),
            new Vector4(-2 * ny * nx, 1 - 2 * ny * ny, -2 * ny * nz, 0),
            new Vector4(-2 * nz * nx, -2 * nz * ny, 1 - 2 * nz * nz, 0),
            new Vector4(0, 0, 0, 1)
        );
    }

    static Vector3 RoundVec(Vector3 v, float tol)
    {
        float f = 1f / tol;
        return new Vector3(
            Mathf.Round(v.x * f) / f,
            Mathf.Round(v.y * f) / f,
            Mathf.Round(v.z * f) / f
        );
    }
}
#endif
