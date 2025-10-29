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
        public List<ModelConfig> levels;
        public ModelSO GetModelSO(ModelType modelType)
        {
            foreach (var item in levels)
            {
                if (item.modelType == modelType)
                    return item.modelSO;
            }
            return null;
        }
        [System.Serializable]
        public class ModelConfig
        {
            public ModelType modelType;
            public ModelSO modelSO;
        }
    }
}