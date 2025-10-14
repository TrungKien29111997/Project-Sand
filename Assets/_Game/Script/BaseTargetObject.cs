using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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

        void OnDrawGizmos()
        {
            if (arrItemDissolve == null) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < arrItemDissolve.Length; i++)
            {
                Gizmos.DrawSphere(arrItemDissolve[i].itemDissolve.transform.position, 0.05f);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green;
                style.fontSize = 30;
                UnityEditor.Handles.Label(arrItemDissolve[i].itemDissolve.transform.position + Vector3.up * 0.2f, arrItemDissolve[i].id.ToString(), style);
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