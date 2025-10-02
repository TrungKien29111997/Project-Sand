using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Config/GraphSO")]
public class ConfigGraphSO : ScriptableObject
{
    public List<ConfigModel> nodes = new List<ConfigModel>();
    [Sirenix.OdinInspector.ReadOnly] public List<EdgeData> edges = new List<EdgeData>();
    public event Action<int> OnNodeSelected;
    public void InvokeNodeSelectedAction(int nodeId) => OnNodeSelected?.Invoke(nodeId);

    [System.Serializable]
    public class ConfigModel
    {
        public int id;
        public List<int> listChildrenId = new List<int>();
        [Sirenix.OdinInspector.ReadOnly] public Vector2 position;
        [Sirenix.OdinInspector.ReadOnly] public string title;
    }

    [System.Serializable]
    public class EdgeData
    {
        public int parentId;
        public int childId;
    }

#if UNITY_EDITOR

    [Button]
    private void OpenGraph()
    {
        //ConfigGraphWindow.Open(this);
    }
#endif
}
