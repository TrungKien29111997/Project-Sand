using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace TrungKien.Core.VFX.Sand
{
    public class SandVFX : PoolingElement
    {
        [SerializeField] VisualEffect vfx;
        [SerializeField] MyVFXTransformBinder[] arrTransformBinder;
        Dictionary<string, MyVFXTransformBinder> dicTransformBinder;
        SandLine sandLine;
        void Awake()
        {
            dicTransformBinder = new();
            for (int i = 0; i < arrTransformBinder.Length; i++)
            {
                dicTransformBinder.Add(arrTransformBinder[i].Property, arrTransformBinder[i]);
            }
        }
        public void SetUp(Color sandColor, int spawnFator, Mesh mesh, float minHeight, float maxHeight, Transform objectTransform, MeshFilter partMeshFilter, Transform target, System.Action callBack = null)
        {
            float dissolveFactor = (maxHeight - minHeight) / 1;
            vfx.SetVector4(Constants.pStartColor, (Vector4)sandColor.linear);
            vfx.SetInt(Constants.pSpawnCount, spawnFator);
            vfx.SetMesh(Constants.pMesh, mesh);
            vfx.SetFloat(Constants.pMaxHeight, maxHeight);
            vfx.SetFloat(Constants.pVFXSandDelayEachLayer, DataSystem.Instance.gameplaySO.delayFactor / dissolveFactor);
            vfx.SetFloat(Constants.pDelay, 1);
            Vector2 randomGravitySpeed = DataSystem.Instance.gameplaySO.gravity;
            vfx.SetVector2(Constants.pRandomGravitySpeed, randomGravitySpeed);
            float delay = minHeight / Mathf.Abs(Random.Range(randomGravitySpeed.x, randomGravitySpeed.y)) + (maxHeight - minHeight) * (DataSystem.Instance.gameplaySO.delayFactor / dissolveFactor) + 1;
            vfx.SetFloat(Constants.pDelayTime, delay);
            vfx.SetVector2(Constants.pLifeTime, new Vector2(delay + 1.5f, delay + 2f));
            dicTransformBinder[Constants.pTranActiveVFXSand].Target = objectTransform;
            dicTransformBinder[Constants.pTranTargetVFXSand].Target = target;
            BuildTriangleBuffer(mesh);
            vfx.Play();
            StartCoroutine(IEDestroy(callBack));
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0, 1),
                new Keyframe(delay / (delay + 1.75f), 1),
                new Keyframe(1, 0)
            );
            vfx.SetAnimationCurve(Constants.pSizeOverLife, curve);

            Bounds b = partMeshFilter.sharedMesh.bounds;
            Vector3 size = Vector3.Scale(b.size, partMeshFilter.transform.lossyScale);
            Fix.DelayedCall(delay, () =>
            {
                sandLine = PoolingSystem.Spawn(DataSystem.Instance.vfxSO.dicPrefabVFX[ETypeVFX.Sand][2]) as SandLine;
                Vector3 worldCenter = partMeshFilter.transform.TransformPoint(partMeshFilter.sharedMesh.bounds.center);
                worldCenter.y = 0;
                sandLine.SetUp(worldCenter, target, Mathf.Sqrt(size.x * size.x + size.z * size.z), sandColor);
            });
        }
        IEnumerator IEDestroy(System.Action callBack)
        {
            yield return new WaitUntil(() => vfx.aliveParticleCount == 0);
            callBack?.Invoke();
            ++LevelControl.Instance.ItemCounter;
            //EventManager.EmitEvent(Constants.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER);
            PoolingSystem.Despawn(this);
            vfx.Stop();
            sandLine.Despawn();
            PoolingSystem.Despawn(sandLine);
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