using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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
        Transform pos0, pos1, pos2, pos3;
        void Update()
        {
            if (sandLine == null) return;
            vfx.SetVector3("Pos0", pos0.position);
            vfx.SetVector3("Pos1", pos1.position + Vector3.up * 0.5f);
            vfx.SetVector3("Pos2", pos2.position + Vector3.up * 0.5f);
            vfx.SetVector3("Pos3", pos3.position);
        }
        public void SetUp(Color sandColor, int spawnFator, Mesh mesh, float minHeight, float maxHeight, Transform objectTransform, MeshFilter partMeshFilter, Transform target, System.Action callBack = null)
        {
            float dissolveFactor = (maxHeight - minHeight) / 1;
            vfx.SetVector4(Constants.pStartColor, (Vector4)sandColor.linear);
            vfx.SetInt(Constants.pSpawnCount, spawnFator);
            vfx.SetMesh(Constants.pMesh, mesh);
            vfx.SetFloat(Constants.pMaxHeight, maxHeight);
            vfx.SetFloat(Constants.pVFXSandDelayEachLayer, DataSystem.Instance.gameplaySO.delayFactor / dissolveFactor);
            vfx.SetFloat(Constants.pDelay, DataSystem.Instance.gameplaySO.timeDelayStartDropSand);
            Vector2 randomGravitySpeed = DataSystem.Instance.gameplaySO.gravity;
            vfx.SetVector2(Constants.pRandomGravitySpeed, randomGravitySpeed);
            float delay = minHeight / Mathf.Abs(Random.Range(randomGravitySpeed.x, randomGravitySpeed.y))
             + (maxHeight - minHeight) * (DataSystem.Instance.gameplaySO.delayFactor / dissolveFactor) + DataSystem.Instance.gameplaySO.timeSandStayInGroud;
            vfx.SetFloat(Constants.pDelayTime, delay);
            vfx.SetFloat(Constants.pLifeTime, delay + DataSystem.Instance.gameplaySO.timeSandFlyOut);
            dicTransformBinder[Constants.pTranActiveVFXSand].Target = objectTransform;
            BuildTriangleBuffer(mesh);
            vfx.Play();
            StartCoroutine(IEDestroy(callBack));
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0, 1),
                new Keyframe(delay / (delay + DataSystem.Instance.gameplaySO.timeSandFlyOut), 1),
                new Keyframe(1, 0)
            );

            // --- Ép tangent linear cho key thứ 2 và 3 ---
            for (int i = 0; i < curve.keys.Length; i++)
            {
                Keyframe k = curve.keys[i];
                if (i == 1) // key thứ 2
                {
                    float dx = curve.keys[2].time - k.time;
                    float dy = curve.keys[2].value - k.value;
                    float m = dy / dx;
                    k.outTangent = m;
                }
                else if (i == 2) // key thứ 3
                {
                    float dx = k.time - curve.keys[1].time;
                    float dy = k.value - curve.keys[1].value;
                    float m = dy / dx;
                    k.inTangent = m;
                }
                curve.MoveKey(i, k);
            }

            vfx.SetAnimationCurve(Constants.pSizeOverLife, curve);

            Bounds b = partMeshFilter.sharedMesh.bounds;
            Vector3 size = Vector3.Scale(b.size, partMeshFilter.transform.lossyScale);
            Fix.DelayedCall(delay - 0.5f, () =>
            {
                sandLine = PoolingSystem.Spawn(DataSystem.Instance.vfxSO.dicPrefabVFX[ETypeVFX.Sand][2]) as SandLine;
                pos0 = sandLine.listTranPoint[0];
                pos3 = sandLine.listTranPoint[^1];
                int indexSpace = sandLine.listTranPoint.Count / 3;
                pos1 = sandLine.listTranPoint[indexSpace];
                pos2 = sandLine.listTranPoint[sandLine.listTranPoint.Count - indexSpace];

                Vector3 worldCenter = partMeshFilter.transform.TransformPoint(partMeshFilter.sharedMesh.bounds.center);
                worldCenter.y = 0;
                sandLine.SetUp(worldCenter, Vector3.up, target, Mathf.Min(size.x, size.z), sandColor, DataSystem.Instance.gameplaySO.timeSandFlyOut + 0.5f);
            });
        }
        // IEnumerator IEDestinationMove()
        // {
        //     float moveDuration = 0.15f;

        //     for (int i = 0; i < sandLine.listTranPoint.Count; i++)
        //     {
        //         float t = 0f;
        //         while (t < 1f)
        //         {
        //             t += Time.deltaTime / moveDuration;

        //             // Vị trí target có thể đã thay đổi nên luôn lấy lại
        //             Vector3 targetPos = sandLine.listTranPoint[i].position;

        //             destinationSandFly.position = Vector3.Lerp(
        //                 destinationSandFly.position,
        //                 targetPos,
        //                 t
        //             );

        //             yield return null;
        //         }
        //     }
        // }
        IEnumerator IEDestroy(System.Action callBack)
        {
            yield return new WaitUntil(() => vfx.aliveParticleCount == 0);
            callBack?.Invoke();
            PoolingSystem.Despawn(this);
            vfx.Stop();
            PoolingSystem.Despawn(sandLine);
            sandLine = null;
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
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (pos0 != null) Gizmos.DrawSphere(pos0.position, 0.15f);
            if (pos1 != null) Gizmos.DrawSphere(pos1.position, 0.15f);
            if (pos2 != null) Gizmos.DrawSphere(pos2.position, 0.15f);
            if (pos3 != null) Gizmos.DrawSphere(pos3.position, 0.15f);
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