using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TrungKien.Core.VFX;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace TrungKien.Core.Gameplay
{
    public abstract class BaseTargetObject : PoolingElement
    {
        [field: SerializeField] public float LocalScale { get; private set; } = 1;
        public ItemDissolveData[] arrItemDissolve;
#if UNITY_EDITOR
        [Button]
        void TestAnimBegin()
        {
            AnimStartLevel(null);
        }
#endif
        public abstract void AnimStartLevel(System.Action callback);

        public void LoadColor(Dictionary<int, PartConfig> dicColor)
        {
            Dictionary<string, int> dicInput = new();
            foreach (var item in dicColor)
            {
                item.Value.listPart.ForEach(x => dicInput.Add(x, item.Key));
            }
            arrItemDissolve.ForEach(x =>
            {
                int idColor = dicInput[x.itemDissolve.gameObject.name];
                x.itemDissolve.SetColor(dicColor[idColor].color);
            });
        }
        public void LoadMaterial(Material mat)
        {
            arrItemDissolve.ForEach(x => x.itemDissolve.meshRen.sharedMaterial = mat);
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
        public Dictionary<int, PartConfig> GetColor()
        {
            BaseDissolveItem[] baseDissolveItem = GetComponentsInChildren<BaseDissolveItem>();
            Dictionary<int, PartConfig> dic = new();
            Dictionary<Material, List<string>> dicCache = new();
            for (int i = 0; i < baseDissolveItem.Length; i++)
            {
                if (!dicCache.ContainsKey(baseDissolveItem[i].meshRen.sharedMaterial))
                {
                    dicCache.Add(baseDissolveItem[i].meshRen.sharedMaterial, new List<string>());
                }
                dicCache[baseDissolveItem[i].meshRen.sharedMaterial].Add(baseDissolveItem[i].gameObject.name);
            }
            int index = 0;
            foreach (var item in dicCache)
            {
                dic.Add(index, new PartConfig()
                {
                    idColor = index,
                    color = item.Key.GetColor(Constants.pShaderSandColor),
                    listPart = new List<string>(item.Value),
                });
                index++;
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