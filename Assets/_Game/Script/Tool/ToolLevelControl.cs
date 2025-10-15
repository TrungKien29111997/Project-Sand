using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace TrungKien.Tool
{
#if UNITY_EDITOR
    public class ToolLevelControl : Singleton<ToolLevelControl>
    {
        [SerializeField] Material matPlane;

        [Button]
        void GenerateMaterial()
        {
            
        }
    }
    #endif
}