using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TrungKien.UI
{
    public class UIManager : Singleton<UIManager>
    {
        Dictionary<System.Type, UICanvas> canvasActives = new();
        Dictionary<System.Type, UICanvas> canvasPrefabs = new();

        public void OnInit()
        {
            Parent ??= GameObject.Find("Canvas - Main").transform;
        }

        public Transform Parent;
        public void Awake()
        {
            DataSystem.Instance.uiSO.prefabCanvas.ForEach(x =>
            {
                canvasPrefabs.Add(x.GetType(), x);
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
        public UICanvas OpenUI(System.Type type)
        {
            if (!(canvasActives.ContainsKey(type) && canvasActives[type] != null))
            {
                canvasActives[type] = Instantiate(canvasPrefabs[type], Parent);
            }
            UICanvas canvas = canvasActives[type];
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
            return canvasActives.ContainsKey(typeof(T)) && canvasActives[typeof(T)] != null;
        }

        // kiem tra canvas duoc active chua
        public bool IsOpened<T>(out T canvas) where T : UICanvas
        {
            if (IsLoaded<T>() && canvasActives[typeof(T)].gameObject.activeSelf)
            {
                canvas = canvasActives[typeof(T)] as T;
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
                T prefab = GetUIPrefab<T>();
                T canvas = Instantiate(prefab, Parent);
                canvasActives[typeof(T)] = canvas;
            }
            return canvasActives[typeof(T)] as T;
        }

        // get Prefabs
        T GetUIPrefab<T>() where T : UICanvas
        {
            return canvasPrefabs[typeof(T)] as T;
        }
        public UICanvas GetUIPrefab(System.Type type)
        {
            return canvasPrefabs[type];
        }

        // dong tat ca
        public void CloseAll()
        {
            foreach (var canvas in canvasActives)
            {
                if (canvas.Value != null && canvas.Value.gameObject.activeSelf)
                {
                    canvas.Value.Close();
                }
            }
        }
        public void CloseChildren(System.Type type)
        {
            foreach (var canvas in canvasActives)
            {
                if (canvas.Value != null && canvas.Value.gameObject.activeSelf && canvas.Value.typeParent == type)
                {
                    canvas.Value.Close();
                }
            }
        }
        public void ReleaseAll()
        {
            foreach (var canvas in canvasActives)
            {
                if (canvas.Value != null)
                {
                    Destroy(canvas.Value.gameObject);
                }
            }
            canvasActives.Clear();
        }

    }
}