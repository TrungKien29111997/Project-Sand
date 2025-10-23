using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TrungKien.Core.Gameplay
{
    [CreateAssetMenu(fileName = "ModelSO", menuName = "TrungKien/ModelSO")]
    public class ModelSO : SerializedScriptableObject
    {
        [PreviewField(120)] public Sprite icon;
        public float timeLimit = 300f;
        public int layerPerPart = 3;
        //public int amountColor;
        public Color colorBG, colorPlane;
        public BaseTargetObject model;
        public Dictionary<int, PartConfig> dicColor;
    }
    [System.Serializable]
    public class PartConfig
    {
        public int idColor;
        public Color color;
        public List<string> listPart;
    }
}