using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrungKien.UI
{
    public class CanvasGamePlay : UICanvas
    {
        [SerializeField] TextMeshProUGUI txtAmount;
        [SerializeField] Scrollbar scrollbar;
        CameraControl cameraControl;
        [SerializeField] float rangeZoom = 20f;
        [SerializeField] RectTransform rectSandBowlGroup, rectSandCacheBowlGroup;
        public Collider colPanelScore { get; private set; }
        [SerializeField] ParticleUI particleUI;
        List<UISandBowl> listUISandBowl;
        List<UICacheSandBowl> listUICacheSandBowl;
        bool isSpawnBowl;
        void Start()
        {
            cameraControl = LevelControl.Instance.cameraCtrl;
            colPanelScore = LevelControl.Instance.colPanelScore;
            scrollbar.onValueChanged.AddListener(CamZoom);
        }

        public void SetSandBow(List<BowlClass> listBowlClass)
        {
            listUISandBowl = new List<UISandBowl>();
            for (int i = 0; i < 4; i++)
            {
                UISandBowl uiBowl = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicUIGameplay[EUIGameplayPool.UISandBowl], default, default, rectSandBowlGroup) as UISandBowl;
                if (i < listBowlClass.Count)
                {
                    uiBowl.SetUp(listBowlClass[i].idColor, listBowlClass[i].GetColor());
                    uiBowl.SetLock(false);
                }
                else
                {
                    uiBowl.SetUp(-1, Color.white);
                    uiBowl.SetLock(true);
                }
                listUISandBowl.Add(uiBowl);
            }
            isSpawnBowl = true;
        }
        public void SetCacheSandBowl(int amount)
        {
            listUICacheSandBowl = new();
            for (int i = 0; i < amount; i++)
            {
                UICacheSandBowl cacheSandBowl = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicUIGameplay[EUIGameplayPool.UICacheSandBowl], default, default, rectSandCacheBowlGroup) as UICacheSandBowl;
                cacheSandBowl.Init();
                listUICacheSandBowl.Add(cacheSandBowl);
            }
        }
        public override void SetUp()
        {
            base.SetUp();
            EventManager.StartListening(Constant.EVENT_GAMEPLAY_UPDATE_SCORE, UpdateUIItemCounter);
            EventManager.StartListening(Constant.EVENT_GAMEPLAY_NEW_BOWL, UpdateBowl);
        }
        public override void Open()
        {
            base.Open();
        }
        public override void Close()
        {
            base.Close();
            EventManager.StopListening(Constant.EVENT_GAMEPLAY_UPDATE_SCORE, UpdateUIItemCounter);
            EventManager.StopListening(Constant.EVENT_GAMEPLAY_NEW_BOWL, UpdateBowl);
        }

        void UpdateUIItemCounter()
        {
            // txtAmount.text = $"{LevelControl.Instance.ItemCounter}/{LevelControl.Instance.MaxItem}";
            // rectTransScore.DOPunchScale(Vector3.one * 0.2f, 0.8f);
            // particleUI.Active();
            List<BowlClass> listBowl = LevelControl.Instance.listBowl;
            List<BowlCacheClass> listCacheBowl = LevelControl.Instance.listCacheBowl;
            for (int i = 0; i < listBowl.Count; i++)
            {
                listUISandBowl[i].SetSandLevel(listBowl[i].counter, LevelControl.Instance.maxColor);
            }
            for (int i = 0; i < listUICacheSandBowl.Count; i++)
            {
                if (!listCacheBowl[i].isEmpty)
                {
                    listUICacheSandBowl[i].AddItem(listCacheBowl[i].idColor, listCacheBowl[i].GetColor());
                }
                else
                {
                    listUICacheSandBowl[i].RemoveItem();
                }
            }
        }
        void UpdateBowl()
        {
            List<BowlClass> listBowlClass = LevelControl.Instance.listBowl;
            for (int i = 0; i < listBowlClass.Count; i++)
            {
                if (listBowlClass[i].isFull)
                {
                    UISandBowl uiBowl = listUISandBowl[i];
                    BowlClass bowlClass = listBowlClass[i];
                    uiBowl.AnimFlyOut(() =>
                    {
                        uiBowl.SetUp(bowlClass.idColor, bowlClass.GetColor());
                    });
                }
            }
        }
        void CamZoom(float value)
        {
            cameraControl.camera.fieldOfView = cameraControl.defaultSizeCamera - value * rangeZoom;
        }
        void LateUpdate()
        {
            if (isSpawnBowl)
            {
                UpdateTranTarget();
            }
        }
        public void UpdateTranTarget()
        {
            for (int i = 0; i < LevelControl.Instance.currentBowl; i++)
            {
                Ray ray = LevelControl.Instance.cameraCtrl.camera.ScreenPointToRay(listUISandBowl[i].TF.position);
                RaycastHit[] hits = Physics.RaycastAll(ray, 2000f);
                for (int j = 0; j < hits.Length; j++)
                {
                    if (ReferenceEquals(colPanelScore, hits[j].collider))
                    {
                        LevelControl.Instance.listBowl[i].gizmoPos.TF.position = hits[j].point;
                        break;
                    }
                }
            }
            for (int i = 0; i < LevelControl.Instance.listCacheBowl.Count; i++)
            {
                Ray ray = LevelControl.Instance.cameraCtrl.camera.ScreenPointToRay(listUICacheSandBowl[i].TF.position);
                RaycastHit[] hits = Physics.RaycastAll(ray, 2000f);
                for (int j = 0; j < hits.Length; j++)
                {
                    if (ReferenceEquals(colPanelScore, hits[j].collider))
                    {
                        LevelControl.Instance.listCacheBowl[i].gizmoPos.TF.position = hits[j].point;
                        break;
                    }
                }
            }
        }
    }
}