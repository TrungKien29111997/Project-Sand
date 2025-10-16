#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PrefabMeshSizeDebugger : MonoBehaviour
{
    [Button]
    void DebugMeshSizes()
    {
        var filters = GetComponentsInChildren<MeshFilter>(true);
        var uniqueMeshes = new HashSet<Mesh>();

        foreach (var f in filters)
        {
            if (f.sharedMesh != null)
                uniqueMeshes.Add(f.sharedMesh);
        }

        long totalBytes = 0;
        foreach (var mesh in uniqueMeshes)
        {
            long meshSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
            totalBytes += meshSize;
            string path = AssetDatabase.GetAssetPath(mesh);
            Debug.Log($"🧩 Mesh: {mesh.name} ({path}) → {FormatBytes(meshSize)}");
        }

        Debug.Log($"📦 Tổng dung lượng mesh unique: {FormatBytes(totalBytes)} (số mesh: {uniqueMeshes.Count})");
    }

    string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024.0;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
#endif
