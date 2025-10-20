using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TrungKien.Core;
using TrungKien.Core.Gameplay;
using TrungKien.Core.VFX;
using UnityEngine;

namespace TrungKien.Tool
{
#if UNITY_EDITOR
    public class ToolLevelControl : Singleton<ToolLevelControl>
    {
        [FoldoutGroup("Config")][SerializeField] List<Material> listInputMaterial;
        [FoldoutGroup("Config")][SerializeField] Camera cam;
        [FoldoutGroup("Config")][SerializeField] Material matPlane;
        [FoldoutGroup("Config")][SerializeField] ModelSO cacheModelSO;
        [FoldoutGroup("Config")][SerializeField] BaseTargetObject cacheModel;
        [SerializeField] ModelType modelType;
        [SerializeField] Color backGroudColor, planeColor;

        void OnValidate()
        {
            matPlane.SetColor("_BaseColor", planeColor);
            cam.backgroundColor = backGroudColor;
        }

        [Button]
        [GUIColor(1f, 0.85f, 0f)]
        void LoadModel()
        {
            if (cacheModel != null)
            {
                DestroyImmediate(cacheModel.gameObject);
                cacheModel = null;
            }
            cacheModelSO = DataSystem.Instance.levelSO.GetModelSO(modelType);
            cacheModel = Instantiate(cacheModelSO.model, Vector3.zero, Quaternion.identity);
        }

        [Button]
        [GUIColor(0.2f, 1f, 0.3f)]
        void LoadMaterial()
        {
            Dictionary<Color, List<string>> dicColor = cacheModelSO.dicColor;
            Dictionary<Material, List<string>> dicInput = new();
            int index = 0;
            foreach (var item in dicColor)
            {
                dicInput.Add(listInputMaterial[index], item.Value);
                listInputMaterial[index].SetColor(Constants.pShaderSandColor, item.Key);

                string path = UnityEditor.AssetDatabase.GetAssetPath(listInputMaterial[index]);
                if (!string.IsNullOrEmpty(path))
                {
                    UnityEditor.AssetDatabase.RenameAsset(path, $"0_Color_{DateTime.UtcNow.ToString("MM.dd.HH.mm")}_{index}");
                }
                index++;
            }
            for (int i = index; i < listInputMaterial.Count; i++)
            {
                listInputMaterial[i].SetColor(Constants.pShaderSandColor, Color.black);
            }
            cacheModel.EditorLoadColor(dicInput);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        [Button]
        [GUIColor(0.4f, 0.8f, 1f)]
        void GenerateMaterial()
        {
            cacheModelSO.colorBG = cam.backgroundColor;
            cacheModelSO.colorPlane = matPlane.GetColor("_BaseColor");
            Dictionary<Color, List<string>> dicColor = cacheModel.GetColor();
            if (dicColor != null)
            {
                cacheModelSO.dicColor = dicColor;
                Debug.Log("Generate Material Done");
            }
        }
    }
    #endif
}