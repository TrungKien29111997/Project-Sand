using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TrungKien.Core.Gameplay
{
    [CreateAssetMenu(fileName = "ModelSO", menuName = "TrungKien/ModelSO")]
    public class ModelSO : SerializedScriptableObject
    {
        [PreviewField(120)] public SpriteRenderer icon;
        public float timeLimit = 300f;
        public int layerPerPart = 3;
        //public int amountColor;
        public Color colorBG, colorPlane;
        public TargetObject model;
    }
}