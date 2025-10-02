using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
namespace TrungKien
{
    public abstract class BaseDissolveItem : PoolingElement
    {
        [SerializeField] bool isDynamic = false;
        [SerializeField] protected MeshRenderer meshRen;
        [SerializeField] protected MeshFilter meshFilter;
        [SerializeField] protected Collider col;
        public int id { get; set; }
        Material mat;
        Material Mat { get { return mat ??= meshRen.material; } }
        Vector2 minMaxHeight;
        Vector3 itemVector3;
        List<ParticleElement> listVfxSand;
        float area, size, speed;
        void Start()
        {
            listVfxSand = new();
        }
        public void Dissolve()
        {
            col.enabled = false;
            float defaultLerp = Mat.GetFloat("_Lerp");
            DOTween.To(x =>
            {
                Mat.SetFloat("_Lerp", x);
            }, defaultLerp, 0, 0.1f).OnComplete(() =>
            {
                minMaxHeight = Extension.GetMinMaxHeightApprox(meshFilter);
                if (isDynamic)
                {
                    LevelControl.Instance.scaleTime = 0;
                }
                itemVector3 = TF.position;
                Fix.DelayedCall(0.1f, () =>
                {
                    DOTween.To(x =>
                        {
                            Mat.SetFloat(Constant.pMainShaderCutOffHeight, x);
                            itemVector3.y = x;
                            LevelControl.Instance.tranPlane.position = itemVector3;
                            List<Vector3> listVector = Extension.GetIntersectionPoints(meshFilter.sharedMesh, TF, LevelControl.Instance.tranPlane);
                            List<Vector3> evenlySpaced = Extension.ResamplePolygonFixedPoints(listVector, 4);
                            area = Extension.ApproximatePolygonArea(evenlySpaced, 4);
                            if (listVfxSand.Count == 0)
                            {
                                listVfxSand = new();
                                for (int i = 0; i < 4; i++)
                                {
                                    ParticleElement psElement = PoolingSystem.Spawn(LevelControl.Instance.vfxSand, TF.position, Quaternion.identity);
                                    psElement.SetColor(Mat.GetColor("_SandColor"));
                                    psElement.Play();
                                    listVfxSand.Add(psElement);
                                }
                            }
                            for (int i = 0; i < listVfxSand.Count; i++)
                            {
                                if (evenlySpaced.Count > i)
                                {
                                    listVfxSand[i].TF.position = evenlySpaced[i];
                                    size = (area / 0.5f) * 1f;
                                    speed = (area / 0.5f) * 0.8f;
                                    if (size < 0.3f)
                                    {
                                        size = 0.3f;
                                    }
                                    if (speed < 0.2f)
                                    {
                                        speed = 0.2f;
                                    }
                                    listVfxSand[i].SetSize(size);
                                    listVfxSand[i].SetSpeed(speed);
                                }
                            }
                        }, minMaxHeight.y, minMaxHeight.x, (minMaxHeight.y - minMaxHeight.x) * DataSystem.Instance.gameplaySO.delayFactor).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            gameObject.SetActive(false);
                            for (int i = 0; i < listVfxSand.Count; i++)
                            {
                                listVfxSand[i].Stop();
                            }
                            Fix.DelayedCall(1.4f, () =>
                            {
                                for (int i = 0; i < listVfxSand.Count; i++)
                                {
                                    PoolingSystem.Despawn(listVfxSand[i]);
                                }
                                listVfxSand.Clear();
                                if (isDynamic)
                                {
                                    LevelControl.Instance.scaleTime = 1;
                                }
                            });
                        });
                });
                //SoundManager.Instance.PlaySound(DataSystem.Instance.gameplaySO.sfxBling);
                SandVFX sandFX = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.SandFX]) as SandVFX;
                sandFX.SetUp(Mat.GetColor("_SandColor"), GetSpawmCount(), meshFilter.sharedMesh, minMaxHeight.x, minMaxHeight.y, TF, LevelControl.Instance.TranDestination);
                Debug.Log("Dissolve");
            });
        }
        int GetSpawmCount()
        {
            Bounds b = meshFilter.sharedMesh.bounds;
            Vector3 size = Vector3.Scale(b.size, TF.lossyScale);
            int spawmCount = (int)(size.x * size.y * size.z * DataSystem.Instance.gameplaySO.spawmFactor);
            if (spawmCount < 80) spawmCount = 80;
            Debug.Log($"Spawm count: {spawmCount}");
            return spawmCount;
        }
        public void Warning()
        {
            col.enabled = false;
            Blink(Color.red, 1, 2, 0.25f, () => col.enabled = true);
        }
        void Blink(Color color, float strength, int loop = 2, float time = 0.2f, System.Action doneAction = null)
        {
            Mat.SetColor(Constant.pMainShaderEmissiveColor, color);
            Sequence seq = DOTween.Sequence();
            for (int j = 0; j < loop; j++)
            {
                seq.Append(DOTween.To(x => Mat.SetFloat(Constant.pMainShaderEmissiveStrength, x), 0, strength, time));
                seq.Append(DOTween.To(x => Mat.SetFloat(Constant.pMainShaderEmissiveStrength, x), strength, 0, time));
            }
            Fix.DelayedCall(loop * time * 2 + 1, () => doneAction?.Invoke());
        }
        // void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.green;
        //     if (meshFilter != null && meshFilter.sharedMesh != null)
        //     {
        //         var mesh = meshFilter.sharedMesh;
        //         var bounds = mesh.bounds;

        //         // local to world
        //         Matrix4x4 m = meshFilter.transform.localToWorldMatrix;
        //         Gizmos.matrix = m;
        //         Gizmos.DrawWireCube(bounds.center, bounds.size);
        //         Gizmos.matrix = Matrix4x4.identity; // reset
        //     }
        // }

#if UNITY_EDITOR
        [Button]
        public void Editor()
        {
            meshRen = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            col = GetComponent<Collider>();
        }

#endif
    }
}