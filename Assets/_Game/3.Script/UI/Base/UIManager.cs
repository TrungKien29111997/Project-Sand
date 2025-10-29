using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrungKien.Core.UI
{
    public class UIManager : Singleton<UIManager>
    {
        Dictionary<System.Type, UICanvas> dicCanvasActives = new();
        Dictionary<System.Type, UICanvas> dicCanvasPrefabs = new();

        Transform parent;
        Transform GetParentCanvas()
        {
            if (parent == null)
            {
                parent = GameObject.FindObjectOfType<UIParent>().transform;
            }
            return parent;
        }
        public override void Awake()
        {
            base.Awake();
            DataSystem.Instance.uiSO.prefabCanvas.ForEach(x =>
            {
                dicCanvasPrefabs.Add(x.GetType(), x);
            });
        }
        // open canvas
        public T OpenUI<T>() where T : UICanvas
        {
            T canvas = GetUI<T>();
            canvas.SetUp();
            canvas.Open();
            return canvas;
        }
        //// dong canvas truc tiep
        //public void CloseDirecly<T>() where T : UICanvas
        //{
        //    canvasActives[typeof(T)].CloseDirecly();
        //}

        // kiem tra canvas duoc tai chua
        public bool IsLoaded<T>() where T : UICanvas
        {
            return dicCanvasActives.ContainsKey(typeof(T)) && dicCanvasActives[typeof(T)] != null;
        }

        // kiem tra canvas duoc active chua
        public bool IsOpened<T>(out T canvas) where T : UICanvas
        {
            if (IsLoaded<T>() && dicCanvasActives[typeof(T)].gameObject.activeSelf)
            {
                canvas = dicCanvasActives[typeof(T)] as T;
                return true;
            }
            else
            {
                canvas = null;
                return false;
            }
        }

        // lay active canvas
        public T GetUI<T>() where T : UICanvas
        {
            if (!IsLoaded<T>())
            {
                System.Type type = typeof(T);

                if (!dicCanvasActives.ContainsKey(type))
                {
                    dicCanvasActives.Add(type, null);
                }
                if (dicCanvasActives[type] == null)
                {
                    dicCanvasActives[type] = Instantiate(dicCanvasPrefabs[type], GetParentCanvas());
                }
            }
            return dicCanvasActives[typeof(T)] as T;
        }

        // dong tat ca
        // public void CloseAll()
        // {
        //     foreach (var canvas in canvasActives)
        //     {
        //         if (canvas.Value != null && canvas.Value.gameObject.activeSelf)
        //         {
        //             canvas.Value.Close();
        //         }
        //     }
        // }
        // public void ReleaseAll()
        // {
        //     foreach (var canvas in canvasActives)
        //     {
        //         if (canvas.Value != null)
        //         {
        //             Destroy(canvas.Value.gameObject);
        //         }
        //     }
        //     canvasActives.Clear();
        // }
    }
}