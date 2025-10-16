using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TrungKien.Core.VFX;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace TrungKien
{
    public abstract class BaseTargetObject : PoolingElement
    {
        [field: SerializeField] public float LocalScale { get; private set; } = 1;
        public ItemDissolveData[] arrItemDissolve;
        void Awake()
        {
            arrItemDissolve.ForEach(x =>
            {
                x.itemDissolve.id = x.id;
            });
        }
        public void LoadColor(Dictionary<Color, List<string>> dicColor)
        {
            Dictionary<string, Color> dicInput = new();
            foreach (var item in dicColor)
            {
                item.Value.ForEach(x => dicInput.Add(x, item.Key));
            }
            arrItemDissolve.ForEach(x =>
            {
                x.itemDissolve.SetColor(dicInput[x.itemDissolve.gameObject.name]);
            });
        }
#if UNITY_EDITOR
        [Button]
        void Editor()
        {
            if (arrItemDissolve.IsNullOrEmpty())
            {
                BaseDissolveItem[] arrItem = GetComponentsInChildren<BaseDissolveItem>();
                arrItemDissolve = new ItemDissolveData[arrItem.Length];
                for (int i = 0; i < arrItem.Length; i++)
                {
                    arrItemDissolve[i] = new ItemDissolveData
                    {
                        id = i,
                        itemDissolve = arrItem[i]
                    };
                }
            }
        }
        [Button]
        void SetUpItemDissolve()
        {
            arrItemDissolve.ForEach(x => x.itemDissolve.Editor());
        }

        // void OnDrawGizmos()
        // {
        //     if (arrItemDissolve == null) return;
        //     Gizmos.color = Color.yellow;
        //     for (int i = 0; i < arrItemDissolve.Length; i++)
        //     {
        //         Gizmos.DrawSphere(arrItemDissolve[i].itemDissolve.transform.position, 0.05f);

        //         GUIStyle style = new GUIStyle();
        //         style.normal.textColor = Color.green;
        //         style.fontSize = 30;
        //         UnityEditor.Handles.Label(arrItemDissolve[i].itemDissolve.transform.position + Vector3.up * 0.2f, arrItemDissolve[i].id.ToString(), style);
        //     }
        // }
        public Dictionary<Color, List<string>> GetColor()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            Dictionary<Color, List<string>> dic = new();
            Dictionary<Material, List<string>> dicCache = new();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (!dicCache.ContainsKey(meshRenderers[i].sharedMaterial))
                {
                    dicCache.Add(meshRenderers[i].sharedMaterial, new List<string>());
                }
                dicCache[meshRenderers[i].sharedMaterial].Add(meshRenderers[i].gameObject.name);
            }
            foreach (var item in dicCache)
            {
                dic.Add(item.Key.GetColor(Constants.pShaderSandColor), item.Value);
            }
            if (dic.Keys.Count <= 1)
            {
                Debug.LogError("Không được để model chỉ có 1 màu");
                return null;
            }
            return dic;
        }

        public void EditorLoadColor(Dictionary<Material, List<string>> dicColor)
        {
            Dictionary<string, Material> dicInput = new();
            foreach (var item in dicColor)
            {
                item.Value.ForEach(x => dicInput.Add(x, item.Key));
            }
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (dicInput.ContainsKey(meshRenderers[i].gameObject.name))
                {
                    meshRenderers[i].sharedMaterial = dicInput[meshRenderers[i].gameObject.name];
                }
            }
        }
#endif
    }
    [System.Serializable]
    public class ItemDissolveData
    {
        public int id;
        public BaseDissolveItem itemDissolve;
        public int[] childrenIDs;
    }
}