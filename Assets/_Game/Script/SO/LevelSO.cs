using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace TrungKien.Core.Gameplay
{
    [CreateAssetMenu(fileName = "LevelSO", menuName = "TrungKien/LevelSO")]
    public class LevelSO : SerializedScriptableObject
    {
        public List<ModelSO> levels;
    }
}