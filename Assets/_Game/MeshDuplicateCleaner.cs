using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshDuplicateCleaner : EditorWindow
{
    [MenuItem("Tools/Clean Duplicate Meshes")]
    static void ShowWindow()
    {
        GetWindow<MeshDuplicateCleaner>("Mesh Cleaner");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scan Project for Duplicate Meshes"))
        {
            CleanMeshes();
        }
    }

    static void CleanMeshes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Mesh");
        var meshDict = new Dictionary<string, Mesh>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

            string hash = GetMeshHash(mesh);

            if (meshDict.ContainsKey(hash))
            {
                Debug.Log($"Duplicate found: {path} is duplicate of {AssetDatabase.GetAssetPath(meshDict[hash])}");
                // ❌ ở đây bạn có thể xóa mesh hoặc thay thế reference bằng meshDict[hash]
            }
            else
            {
                meshDict.Add(hash, mesh);
            }
        }

        Debug.Log("Scan complete!");
    }

    static string GetMeshHash(Mesh mesh)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        // Tạo chuỗi hash đơn giản (có thể thay bằng MD5/SHA1 nếu muốn)
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + verts.Length;
            hash = hash * 31 + tris.Length;

            for (int i = 0; i < verts.Length; i++)
            {
                hash = hash * 31 + verts[i].GetHashCode();
            }
            for (int i = 0; i < tris.Length; i++)
            {
                hash = hash * 31 + tris[i];
            }

            return hash.ToString();
        }
    }
}
