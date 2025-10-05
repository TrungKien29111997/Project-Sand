using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace TrungKien.Core.VFX
{
    [CreateAssetMenu(fileName = "VFXSO", menuName = "TrungKien/VFXSO")]
    public class VFXSO : SerializedScriptableObject
    {
        public Dictionary<ETypeVFX, List<PoolingElement>> dicPrefabVFX;
    }
}