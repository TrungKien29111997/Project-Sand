using System.Collections;
using System.Collections.Generic;
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
        public Transform tarGetFly { get; private set; }
        [SerializeField] RectTransform rectTransScore;
        public Collider colPanelScore { get; private set; }
        [SerializeField] ParticleUI particleUI;
        void Start()
        {
            cameraControl = LevelControl.Instance.cameraCtrl;
            tarGetFly = LevelControl.Instance.TranDestination;
            colPanelScore = LevelControl.Instance.colPanelScore;
            scrollbar.onValueChanged.AddListener(CamZoom);
        }
        public override void SetUp()
        {
            base.SetUp();
            EventManager.StartListening(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER, UpdateUIItemCounter);
        }
        public override void Close()
        {
            base.Close();
            EventManager.StopListening(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER, UpdateUIItemCounter);
        }
        void UpdateUIItemCounter()
        {
            txtAmount.text = $"{LevelControl.Instance.ItemCounter}/{LevelControl.Instance.MaxItem}";
            rectTransScore.DOPunchScale(Vector3.one * 0.2f, 0.8f);
            particleUI.Active();
        }
        void CamZoom(float value)
        {
            cameraControl.camera.fieldOfView = cameraControl.defaultSizeCamera - value * rangeZoom;
        }
        void LateUpdate()
        {
            UpdateTranTarget();
        }
        public void UpdateTranTarget()
        {
            Ray ray = LevelControl.Instance.cameraCtrl.camera.ScreenPointToRay(rectTransScore.position);
            RaycastHit[] hits = Physics.RaycastAll(ray, 2000f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (ReferenceEquals(colPanelScore, hits[i].collider))
                {
                    tarGetFly.position = hits[i].point;
                    break;
                }
            }
        }
    }
}