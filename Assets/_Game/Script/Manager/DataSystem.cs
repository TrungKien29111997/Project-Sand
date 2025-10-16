using System.Collections;
using System.Collections.Generic;
using TrungKien.Core.Gameplay;
using TrungKien.Core.VFX;
using UnityEngine;
namespace TrungKien
{
    public class DataSystem : Singleton<DataSystem>
    {
        public PrefabConfigsSO prefabSO;
        public GameplaySO gameplaySO;
        public UISO uiSO;
        public MaterialSO materialSO;
        public VFXSO vfxSO;
        public LevelSO levelSO;
    }
}