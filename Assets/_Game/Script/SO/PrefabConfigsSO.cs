using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TrungKien
{
    [CreateAssetMenu(fileName = "PrefabConfigSO", menuName = "TrungKien/PrefabConfigSO")]
    public class PrefabConfigsSO : SerializedScriptableObject
    {
        [FoldoutGroup("Dictionary")]
        public Dictionary<EPooling, PoolingElement> dicObjPooling;
    }
}
