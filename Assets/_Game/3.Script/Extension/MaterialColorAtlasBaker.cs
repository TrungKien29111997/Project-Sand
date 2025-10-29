using UnityEngine;
using System.Collections.Generic;
namespace TrungKien
{
#if  UNITY_EDITOR
    public class MaterialColorAtlasBaker : MonoBehaviour
    {
        [UnityEditor.MenuItem("Tools/Bake BaseColor Atlas")]
        static void Bake()
        {
            var go = UnityEditor.Selection.activeGameObject;
            if (go == null) return;

            var mf = go.GetComponent<MeshFilter>();
            var mr = go.GetComponent<MeshRenderer>();
            if (mf == null || mr == null) return;

            var mesh = Object.Instantiate(mf.sharedMesh);
            var mats = mr.sharedMaterials;

            int count = mats.Length;
            int atlasCols = Mathf.CeilToInt(Mathf.Sqrt(count));
            int atlasRows = Mathf.CeilToInt((float)count / atlasCols);

            // tạo texture atlas
            int texSize = 256;
            Texture2D atlas = new Texture2D(atlasCols, atlasRows);
            Color[] pixels = new Color[atlasCols * atlasRows];
            for (int i = 0; i < count; i++)
            {
                Color c = Color.white;
                if (mats[i].HasProperty("_BaseColor"))
                    c = mats[i].GetColor("_BaseColor");

                int x = i % atlasCols;
                int y = i / atlasCols;
                pixels[y * atlasCols + x] = c;
            }
            atlas.SetPixels(pixels);
            atlas.Apply();

            // phóng to lên để dùng như texture
            Texture2D finalTex = new Texture2D(texSize * atlasCols, texSize * atlasRows);
            for (int y = 0; y < atlasRows; y++)
            {
                for (int x = 0; x < atlasCols; x++)
                {
                    Color c = pixels[y * atlasCols + x];
                    Color[] block = new Color[texSize * texSize];
                    for (int j = 0; j < block.Length; j++) block[j] = c;
                    finalTex.SetPixels(x * texSize, y * texSize, texSize, texSize, block);
                }
            }
            finalTex.Apply();

            // chỉnh UV cho mỗi submesh
            Vector2[] uvs = new Vector2[mesh.vertexCount];
            mesh.GetUVs(0, new System.Collections.Generic.List<Vector2>(uvs));
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var indices = mesh.GetIndices(i);
                int x = i % atlasCols;
                int y = i / atlasCols;

                float uMin = (float)x / atlasCols;
                float vMin = (float)y / atlasRows;
                float uMax = (float)(x + 1) / atlasCols;
                float vMax = (float)(y + 1) / atlasRows;

                foreach (var idx in indices)
                    uvs[idx] = new Vector2((uMin + uMax) * 0.5f, (vMin + vMax) * 0.5f);
            }
            mesh.SetUVs(0, new System.Collections.Generic.List<Vector2>(uvs));

            // gộp submesh thành 1
            mesh.subMeshCount = 1;
            mesh.SetTriangles(mesh.triangles, 0);

            // tạo material mới
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetTexture("_BaseMap", finalTex);

            // gán lại
            mf.sharedMesh = mesh;
            mr.sharedMaterial = mat;

            // lưu asset
            UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/BakedMesh.asset");
            UnityEditor.AssetDatabase.CreateAsset(finalTex, "Assets/BakedAtlas.asset");
            UnityEditor.AssetDatabase.CreateAsset(mat, "Assets/BakedMat.mat");
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log("Bake done!");
        }
    }
    #endif
}