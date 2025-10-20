using System.Collections;
using System.Collections.Generic;
using TrungKien.Core.VFX;
using UniRx;
using UnityEngine;

namespace TrungKien.Core
{
    public class GameManager : Singleton<GameManager>
    {
        [field: SerializeField] public EGameState GameState { get; private set; }
        Vector2 screenSize;
        public bool Initilized { get; private set; }
        private bool isFirstTimeInit = true;

        public override void Awake()
        {
            base.Awake();
            PoolingSystem.Init();
            VFXSystem.Init();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 1000;
            bool enabled = UnityEngine.Rendering.GraphicsSettings.useScriptableRenderPipelineBatching;
            Debug.Log("🔍 SRP Batcher: " + (enabled ? "ENABLED ✅" : "DISABLED ❌"));
        }
        private void Start()
        {
            screenSize = new Vector2(Screen.width, Screen.height);
            StartCoroutine(IEInit());
        }
        IEnumerator IEInit()
        {
            DebugCustom.LogColor("Wait loading panel");
            yield return null;
            yield return new WaitUntil(() => LoadingPanel.Instance);
            yield return new WaitUntil(() => DataSystem.Instance);
            
            yield return StartCoroutine(IELoading());

            // yield return StartCoroutine(ITimerController.Instance.IEInit());
            // yield return StartCoroutine(IArtifactController.Instance.IEInit());
            // yield return StartCoroutine(IAchievementController.Instance.IEInit());
            // yield return StartCoroutine(IEquipmentController.Instance.IEInit());
            // yield return StartCoroutine(IGachaController.Instance.IEInit());
            // yield return StartCoroutine(IPlayerResource.Instance.IEInit());
            // yield return StartCoroutine(ILabController.Instance.IEInit());
            // yield return StartCoroutine(IChapterController.Instance.IEInit());
            // yield return StartCoroutine(IWorkShopController.Instance.IEInit());
            // yield return StartCoroutine(IHeroController.Instance.IEInit());
            // yield return StartCoroutine(ICheckinController.Instance.IEInit());
            // yield return StartCoroutine(IIAPController.Instance.IEInit());

            yield return null;
            Initilized = true;
            if (isFirstTimeInit)
            {
                isFirstTimeInit = false;
                Observable.Interval(System.TimeSpan.FromSeconds(1), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ => OnTick()).AddTo(gameObject);
            }
            yield return StartCoroutine(LoadingPanel.Instance.EndLoading());
            GoSceneHome();
        }
        private void OnTick()
        {
            EventManager.EmitEvent(Constant.TIMER_TICK_EVENT);
        }
        IEnumerator IELoading()
        {
            LoadingPanel.Instance.ShowTextLoading("Get Config");
            yield return new WaitUntil(() => LoadingPanel.Instance);
            LoadingPanel.Instance.StartLoading();
        }

        public void SetGameState(EGameState state)
        {
            GameState = state;
        }
        #region Scene Control
        public void GoSceneHome(Transform focusTransform = null)
        {
            DebugCustom.Log("Go Scene Home");
            SetGameState(EGameState.Loading);
            StartCoroutine(SceneHelper.Instance.IEReturnHome(focusTransform));
        }

        public void GoSceneGameplay(Transform focusTransform = null)
        {
            DebugCustom.Log("Go Scene Gameplay");
            SetGameState(EGameState.Loading);
            StartCoroutine(SceneHelper.Instance.IEGoGameplay(focusTransform));
        }
        #endregion
    }
}