using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Config/GraphSO")]
public class ConfigGraphSO : ScriptableObject
{
    [System.Serializable]
    public class ConfigModel
    {
        public int id;
        public List<int> listChildrenId = new List<int>();
        public Vector2 position;
        public string title;
    }

    [System.Serializable]
    public class EdgeData
    {
        public int parentId;
        public int childId;
    }

    public List<ConfigModel> nodes = new List<ConfigModel>();
    public List<EdgeData> edges = new List<EdgeData>();

    // ✅ Mỗi ScriptableObject tự có Action riêng
    public Action<int> OnNodeSelected;

#if UNITY_EDITOR

    [Button]
    private void OpenGraph()
    {
        //ConfigGraphWindow.Open(this);
    }
#endif
}
