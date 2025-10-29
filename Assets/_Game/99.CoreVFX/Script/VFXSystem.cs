using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TrungKien.Core.VFX.Sand;
using Unity.VisualScripting;
using UnityEngine;

namespace TrungKien.Core.VFX
{
    public static class VFXSystem
    {
        static List<MaterialPropertyBlock> listMPB;
        static Dictionary<ETypeVFX, List<Material>> dicMat;
        public static void Init()
        {
            listMPB = new List<MaterialPropertyBlock>();
            dicMat = new Dictionary<ETypeVFX, List<Material>>();
            List<ETypeVFX> listTypeVFX = Extension.GetListEnum<ETypeVFX>();
            listTypeVFX.ForEach(typeVFX => dicMat.Add(typeVFX, new List<Material>()));
        }
        #region Pooling MPB
        public static MaterialPropertyBlock GetMPB()
        {
            if (listMPB.Count == 0)
            {
                listMPB.Add(new MaterialPropertyBlock());
            }
            MaterialPropertyBlock mpb = listMPB[0];
            listMPB.RemoveAt(0);
            return mpb;
        }
        public static void ReturnMPB(MaterialPropertyBlock mpb)
        {
            listMPB.Add(mpb);
        }
        #endregion

        #region Pooling Material
        public static Material GetMaterial(ETypeVFX typeVFX)
        {
            if (dicMat[typeVFX].Count == 0)
            {
                dicMat[typeVFX].Add(new Material(DataSystem.Instance.materialSO.dicMat[typeVFX]));
            }
            Material mat = dicMat[typeVFX][0];
            dicMat[typeVFX].RemoveAt(0);
            return mat;
        }
        public static void ReturnMaterial(ETypeVFX typeVFX, Material mat)
        {
            dicMat[typeVFX].Add(mat);
        }
        #endregion

        public static void SpawnVFX(ETypeVFX typeVFX, Transform TF, Transform targetFly, MeshFilter meshFilter, MeshRenderer meshRen, Color vfxColor, System.Action dissoleDoneAction = null, System.Action addScoreAction = null)
        {
            switch (typeVFX)
            {
                case ETypeVFX.Sand:
                    SandDissolve(TF, targetFly, meshFilter, meshRen, vfxColor, dissoleDoneAction, addScoreAction);
                    break;
            }
        }
        static void SandDissolve(Transform TF, Transform targetFly, MeshFilter meshFilter, MeshRenderer meshRen, Color vfxColor, System.Action dissoleDoneAction, System.Action addScoreAction)
        {
            Vector2 minMaxHeight = Extension.GetMinMaxHeightApprox(meshFilter);
            float dissolveFactor = (minMaxHeight.y - minMaxHeight.x) / 1;
            List<SandSubElement> listVfxSand = new();
            float area, size, speed;
            List<Vector3> listVector, evenlySpaced;
            Transform plane = new GameObject().transform;

            MaterialPropertyBlock mpb = GetMPB();
            mpb.SetColor(Constants.pShaderSandColor, vfxColor);

            SandVFX sandFX = PoolingSystem.Spawn(DataSystem.Instance.vfxSO.dicPrefabVFX[ETypeVFX.Sand][0]) as SandVFX;
            sandFX.SetUp(vfxColor, GetSpawmCount(TF, meshFilter), meshFilter.sharedMesh, minMaxHeight.x, minMaxHeight.y, TF, meshFilter, targetFly, addScoreAction);
            for (int i = 0; i < Constants.spawnSandDrop; i++)
            {
                SandSubElement psElement = PoolingSystem.Spawn(DataSystem.Instance.vfxSO.dicPrefabVFX[ETypeVFX.Sand][1], TF.position, Quaternion.identity) as SandSubElement;
                psElement.SetColor(vfxColor);
                psElement.Play();
                listVfxSand.Add(psElement);
            }
            meshRen.sharedMaterial = GetMaterial(ETypeVFX.Sand);
            // if (isDynamic)
            // {
            //     LevelControl.Instance.scaleTime = 0;
            // }
            Vector3 itemVector3 = TF.position;
            DOTween.To(x =>
                {
                    mpb.SetFloat(Constants.pShaderCutOffHeight, x);
                    meshRen.SetPropertyBlock(mpb);
                    itemVector3.y = x;
                    plane.position = itemVector3;
                    listVector = Extension.GetIntersectionPoints(meshFilter.sharedMesh, TF, plane);
                    evenlySpaced = Extension.ResamplePolygonFixedPoints(listVector, Constants.spawnSandDrop);
                    area = Extension.ApproximatePolygonArea(evenlySpaced, Constants.spawnSandDrop);
                    for (int i = 0; i < listVfxSand.Count; i++)
                    {
                        if (evenlySpaced.Count > i)
                        {
                            listVfxSand[i].TF.position = evenlySpaced[i];
                            size = (area / 0.5f) * 1f;
                            speed = (area / 0.5f) * 0.8f;
                            if (size < 0.3f) size = 0.3f;
                            if (speed < 0.2f) speed = 0.2f;
                            listVfxSand[i].SetSize(size);
                            listVfxSand[i].SetSpeed(speed);
                        }
                    }
                }, minMaxHeight.y, minMaxHeight.x, (((minMaxHeight.y - minMaxHeight.x) * (DataSystem.Instance.gameplaySO.delayFactor)) / dissolveFactor) + 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    GameObject.Destroy(plane.gameObject);
                    dissoleDoneAction?.Invoke();
                    ReturnMPB(mpb);
                    ReturnMaterial(ETypeVFX.Sand, meshRen.sharedMaterial);
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
                        // if (isDynamic)
                        // {
                        //     LevelControl.Instance.scaleTime = 1;
                        // }
                    });
                });

        }
        static int GetSpawmCount(Transform TF, MeshFilter meshFilter)
        {
            Bounds b = meshFilter.sharedMesh.bounds;
            Vector3 size = Vector3.Scale(b.size, TF.lossyScale);
            int spawmCount = (int)(size.x * size.y * size.z * DataSystem.Instance.gameplaySO.spawmFactor);
            if (spawmCount < Constants.minSandSpawnCount)
            {
                spawmCount = Constants.minSandSpawnCount;
            }
            if (spawmCount > Constants.maxScandSpawnCount)
            {
                spawmCount = Constants.maxScandSpawnCount;
            }
            DebugCustom.Log($"Spawm count: {spawmCount}");
            return spawmCount;
        }
    }
}