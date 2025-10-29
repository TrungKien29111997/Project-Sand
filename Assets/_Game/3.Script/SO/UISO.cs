using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TrungKien.Core.UI;
using UnityEngine;
namespace TrungKien
{
    [CreateAssetMenu(fileName = "UISO", menuName = "TrungKien/UISO")]
    public class UISO : SerializedScriptableObject
    {
        public UICanvas[] prefabCanvas;
    }
}