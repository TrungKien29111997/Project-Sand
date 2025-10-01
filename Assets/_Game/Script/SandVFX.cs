using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace TrungKien
{
    public class SandVFX : PoolingElement
    {
        [SerializeField] VisualEffect vfx;
        [SerializeField] MyVFXTransformBinder[] arrTransformBinder;
        Dictionary<string, MyVFXTransformBinder> dicTransformBinder;
        void Awake()
        {
            dicTransformBinder = new();
            for (int i = 0; i < arrTransformBinder.Length; i++)
            {
                dicTransformBinder.Add(arrTransformBinder[i].Property, arrTransformBinder[i]);
            }
        }
        public void SetUp(Color sandColor, int spawnFator, Mesh mesh, float minHeight, float maxHeight, Transform objectTransform, Transform target)
        {
            vfx.SetVector4("StartColor", (Vector4)sandColor.linear);
            vfx.SetInt("SpawnCount", spawnFator);
            vfx.SetMesh(Constant.pMesh, mesh);
            vfx.SetFloat("MaxHeight", maxHeight);
            vfx.SetFloat(Constant.pVFXSandDelayEachLayer, DataSystem.Instance.gameplaySO.delayFactor);
            vfx.SetFloat("Delay", 1);
            float delay = minHeight / 3 + (maxHeight - minHeight) * DataSystem.Instance.gameplaySO.delayFactor + 1 + 1;
            vfx.SetFloat("DelayTime", delay);
            vfx.SetVector2("LifeTime", new Vector2(delay + 1.5f, delay + 3f));
            dicTransformBinder[Constant.pTranActiveVFXSand].Target = objectTransform;
            dicTransformBinder[Constant.pTranTargetVFXSand].Target = target;
            BuildTriangleBuffer(mesh);
            vfx.Play();
            DOTween.To(x => vfx.SetFloat("Alpha", x), 0, 1, 1).SetEase(Ease.Linear);
            StartCoroutine(IEDestroy());
        }
        IEnumerator IEDestroy()
        {
            yield return new WaitUntil(() => vfx.aliveParticleCount == 0);
            ++LevelControl.Instance.ItemCounter;
            EventManager.EmitEvent(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER);
            PoolingSystem.Despawn(this);
            vfx.Stop();
        }
        void BuildTriangleBuffer(Mesh mesh)
        {
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;

            List<int> weightedTriangles = new List<int>();

            // duyệt qua từng tam giác (mỗi 3 chỉ số)
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                // tính diện tích tam giác
                float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;

                // làm cho tỉ lệ xuất hiện tỉ lệ theo diện tích
                int reps = Mathf.Max(1, Mathf.RoundToInt(area * 100f)); // scale 100 có thể chỉnh
                for (int r = 0; r < reps; r++)
                {
                    weightedTriangles.Add(i / 3); // index tam giác
                }
            }

            int[] triangleIndices = weightedTriangles.ToArray();

            // tạo buffer
            GraphicsBuffer triangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                                                triangleIndices.Length, sizeof(int));
            triangleBuffer.SetData(triangleIndices);

            // truyền buffer vào VFX Graph
            vfx.SetGraphicsBuffer("TriangleGraphicsBuffer", triangleBuffer);
            vfx.SetInt("TriangleBufferCount", triangleIndices.Length);
        }
#if UNITY_EDITOR
        [Button]
        void Editor()
        {
            arrTransformBinder = GetComponents<MyVFXTransformBinder>();
        }
#endif
    }
}