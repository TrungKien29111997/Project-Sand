using System.Collections;
using System.Collections.Generic;
using TrungKien.Core.VFX;
using TrungKien.UI;
using UnityEngine;

namespace TrungKien
{
    public class GameManager : Singleton<GameManager>
    {
        void Awake()
        {
            PoolingSystem.Init();
            VFXSystem.Init();
            UIManager.Instance.Init();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 1000;
            bool enabled = UnityEngine.Rendering.GraphicsSettings.useScriptableRenderPipelineBatching;
            Debug.Log("🔍 SRP Batcher: " + (enabled ? "ENABLED ✅" : "DISABLED ❌"));
        }
    }
}