using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TrungKien.Core.VFX
{
    [CreateAssetMenu(fileName = "MaterialSO", menuName = "TrungKien/MaterialSO")]
    public class MaterialSO : SerializedScriptableObject
    {
        public Dictionary<ETypeVFX, Material> dicMat = new();
    }
}