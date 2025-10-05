using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace TrungKien
{
    public static class PoolingSystem
    {
        static Dictionary<int, PoolCtrlObject> dicObjPool;
        public static void Init()
        {
            dicObjPool = new();
        }
        public static T Spawn<T>(T poolObj, Vector3 pos = default, Quaternion rot = default, Transform parent = null) where T : PoolingElement
        {
            int instanceID = poolObj.GetInstanceID();
            if (!dicObjPool.ContainsKey(instanceID))
            {
                dicObjPool.Add(instanceID, new PoolCtrlObject(poolObj));
            }
            return dicObjPool[instanceID].Spawn(pos, rot, parent) as T;
        }
        public static void Despawn(PoolingElement poolObj)
        {
            int instanceID = poolObj.InstanceID;
            if (!dicObjPool.ContainsKey(instanceID))
            {
                Debug.LogError($"{poolObj} with instanceID {instanceID} not load");
                return;
            }
            dicObjPool[instanceID].DeSpawn(poolObj);
        }
        public static void Collect(PoolingElement poolObj)
        {
            int instanceID = poolObj.GetInstanceID();
            if (!dicObjPool.ContainsKey(instanceID))
            {
                Debug.LogError($"{poolObj} with instanceID {instanceID} not load");
                return;
            }
            dicObjPool[instanceID].Collect();
        }
        public static void Release(PoolingElement poolObj)
        {
            int instanceID = poolObj.GetInstanceID();
            if (!dicObjPool.ContainsKey(instanceID))
            {
                Debug.LogError($"{poolObj} with instanceID {instanceID} not load");
                return;
            }
            dicObjPool[instanceID].Release();
        }
        public static void CollectAll()
        {
            foreach (var value in dicObjPool.Values)
            {
                value.Collect();
            }
        }
    }
    public class PoolCtrlObject
    {
        PoolingElement prefab;
        Queue<PoolingElement> queueInActive = new();
        List<PoolingElement> lstActive = new();

        public PoolCtrlObject(PoolingElement prefab)
        {
            this.prefab = prefab;
        }
        public PoolingElement Spawn(Vector3 pos, Quaternion rot, Transform parent = null)
        {
            PoolingElement element = (queueInActive.Count <= 0) ? GameObject.Instantiate(prefab, pos, rot) : queueInActive.Dequeue();
            element.InstanceID = prefab.GetInstanceID();
            element.TF.SetParent(parent);
            if (parent == null)
            {
                element.TF.SetPositionAndRotation(pos, rot);
            }
            else
            {
                element.TF.SetLocalPositionAndRotation(pos, rot);
            }
            element.gameObject.SetActive(true);
            element.TF.localScale = Vector3.one;
            lstActive.Add(element);
            element.PoolSetup();
            return element;
        }
        public void DeSpawn(PoolingElement element)
        {
            if (element != null && element.gameObject.activeSelf)
            {
                lstActive.Remove(element);
                queueInActive.Enqueue(element);
                element.gameObject.SetActive(false);
            }
        }
        // get all pool element to queue inActive 
        public void Collect()
        {
            for (int i = lstActive.Count - 1; i >= 0; i--)
            {
                DeSpawn(lstActive[i]);
            }
        }
        // destroy all element
        public void Release()
        {
            Collect();
            while (queueInActive.Count > 0)
            {
                GameObject.Destroy(queueInActive.Dequeue().gameObject);
            }
            queueInActive.Clear();
        }
    }
}