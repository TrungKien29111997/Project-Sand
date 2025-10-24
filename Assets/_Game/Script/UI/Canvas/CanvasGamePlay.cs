using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using TrungKien.Core.VFX;
using UnityEngine;
using UnityEngine.UI;

namespace TrungKien.Core.UI
{
    public class CanvasGamePlay : UICanvas
    {
        [SerializeField] TextMeshProUGUI txtAmount, txtDebugFPS;
        [SerializeField] Scrollbar scrollbar;
        CameraControl cameraControl;
        [SerializeField] float rangeZoom = 20f;
        [SerializeField] RectTransform rectSandBowlGroup, rectSandCacheBowlGroup;
        public Collider colPanelScore { get; private set; }
        [SerializeField] ParticleUI particleUI;
        public List<UISandBowl> listUISandBowl;
        public List<UICacheSandBowl> listUICacheSandBowl;
        [SerializeField] LayerMask layerUIPlane;
        void Start()
        {
            cameraControl = LevelControl.Instance.cameraCtrl;
            colPanelScore = LevelControl.Instance.colPanelScore;
            scrollbar.onValueChanged.AddListener(CamZoom);
        }

        public TextMeshProUGUI fpsText;
        private float deltaTime = 0.0f;
        float maxFPS = 0;
        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            if (fps > maxFPS) maxFPS = fps;
            fpsText.text = $"{fps:0.} FPS \nMax:<color=green> {maxFPS:0.} </color>";
        }
        public void DebugFPS()
        {
            txtDebugFPS.text = $"{1.0f / deltaTime:0.} FPS";
        }

        public void SetUpSandBowl(List<BowlClass> listBowlClass)
        {
            listUISandBowl = new List<UISandBowl>();
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                UISandBowl uiBowl = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicUIGameplay[EUIGameplayPool.UISandBowl], default, default, rectSandBowlGroup) as UISandBowl;
                uiBowl.indexBowl = index;
                if (index < listBowlClass.Count)
                {
                    uiBowl.SetUp(listBowlClass[index].GetColor());
                    uiBowl.SetLock(false);
                }
                else
                {
                    uiBowl.SetUp(Color.white);
                    uiBowl.SetLock(true);
                }
                listUISandBowl.Add(uiBowl);
            }
        }
        public void SetUpCacheSandBowl(int amount)
        {
            listUICacheSandBowl = new();
            for (int i = 0; i < amount; i++)
            {
                int index = i;
                UICacheSandBowl cacheSandBowl = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicUIGameplay[EUIGameplayPool.UICacheSandBowl], default, default, rectSandCacheBowlGroup) as UICacheSandBowl;
                cacheSandBowl.Init();
                cacheSandBowl.indexCacheBowl = index;
                listUICacheSandBowl.Add(cacheSandBowl);
            }
        }
        public override void SetUp()
        {
            base.SetUp();
        }
        public override void Open()
        {
            base.Open();
        }
        public override void Close()
        {
            base.Close();
        }
        List<ChangeSandBowl> listEffectChangeBowl => LevelControl.Instance.listEffectChangeBowl;
        List<BowlClass> listBowl => LevelControl.Instance.listBowl;
        List<BowlCacheClass> listCacheBowl => LevelControl.Instance.listCacheBowl;
        public void UpdateUICacheBowl()
        {
            // txtAmount.text = $"{LevelControl.Instance.ItemCounter}/{LevelControl.Instance.MaxItem}";
            // rectTransScore.DOPunchScale(Vector3.one * 0.2f, 0.8f);
            // particleUI.Active();

            // for (int i = 0; i < listUICacheSandBowl.Count; i++)
            // {
            //     if (!listCacheBowl[i].isEmpty)
            //     {
            //         listUICacheSandBowl[i].Fill(listCacheBowl[i].GetColor());
            //     }
            //     else
            //     {
            //         listUICacheSandBowl[i].Init();
            //     }
            // }
            if (listEffectChangeBowl.Count > 0)
            {
                listEffectChangeBowl.ForEach(x =>
                {
                    Vector3 posMain = listUISandBowl[x.indexMainBowl].TF.position;
                    Vector3 posCache = listUICacheSandBowl[x.indexCacheBowl].TF.position;
                    SandLine sandLine = PoolingSystem.Spawn(DataSystem.Instance.vfxSO.dicPrefabVFX[VFX.ETypeVFX.Sand][3], posCache, Quaternion.LookRotation((posMain - posCache).normalized, -LevelControl.Instance.cameraCtrl.TF.forward)) as SandLine;
                    sandLine.SetUp(posCache, sandLine.TF.up, listUISandBowl[x.indexMainBowl].TF, 0.5f, LevelControl.Instance.GetColor(x.indexColor), 2f);
                });
                listEffectChangeBowl.Clear();
                Fix.DelayedCall(2.2f, SortCacheBowl);
            }
        }
        void SortCacheBowl()
        {
            StartCoroutine(IEVisualShareBowl());
        }
        IEnumerator IEVisualShareBowl()
        {
            List<int> emptyIndexBowl = LevelControl.Instance.SortCacheBowl();
            for (int i = 0; i < emptyIndexBowl.Count; i++)
            {
                int index = i;
                if (emptyIndexBowl[index] == 0)
                {

                    if (index < listUICacheSandBowl.Count - 1)
                    {
                        int indexCacheBowlHaveSand = GetIndexCacheBowlHaveSand(emptyIndexBowl, index);
                        if (indexCacheBowlHaveSand != -1)
                        {
                            UICacheSandBowl currentEmptySandBowl = listUICacheSandBowl[index];
                            UICacheSandBowl fillSandBowl = listUICacheSandBowl[indexCacheBowlHaveSand];
                            yield return fillSandBowl.IEShareSandToPreviousCacheBowl(currentEmptySandBowl.TF.position,() =>
                            {
                                currentEmptySandBowl.Fill(listCacheBowl[index].GetColor());
                                fillSandBowl.Init();
                            });
                            emptyIndexBowl[index] = 1;
                            emptyIndexBowl[indexCacheBowlHaveSand] = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        int GetIndexCacheBowlHaveSand(List<int> emptyIndexBowl, int startIndex)
        {
            if (startIndex >= emptyIndexBowl.Count) return -1;
            for (int i = startIndex; i < emptyIndexBowl.Count; i++)
            {
                if (emptyIndexBowl[i] == 1)
                {
                    return i;
                }
            }
            return -1;
        }
        void CamZoom(float value)
        {
            cameraControl.camera.fieldOfView = cameraControl.defaultSizeCamera - value * rangeZoom;
        }

        public void WarningFullCacheBowl()
        {
            Blink(Color.red, 1, 2, 0.25f);
        }
        void Blink(Color color, float strength, int loop = 2, float time = 0.2f, System.Action doneAction = null)
        {
            Sequence seq = DOTween.Sequence();
            for (int j = 0; j < loop; j++)
            {
                seq.Append(DOTween.To(x => CacheBowlSetColor(color, x), 0, strength, time));
                seq.Append(DOTween.To(x => CacheBowlSetColor(color, x), strength, 0, time));
            }
            seq.Append(DOTween.To(x => CacheBowlSetColor(Color.white, x), 0, 1, time));
            Fix.DelayedCall(loop * time * 2 + 1, () => doneAction?.Invoke());
        }
        void CacheBowlSetColor(Color color, float alpha)
        {
            listUICacheSandBowl.ForEach(x => x.VisualBowlFade(color, alpha));
        }
    }
}