using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using TrungKien.UI;
namespace TrungKien
{
    public class LevelControl : Singleton<LevelControl>
    {
        public CameraControl cameraCtrl;
        Dictionary<Collider, BaseDissolveItem> dicDissolveItem;
        Vector3 point;
        [SerializeField] BaseTargetObject[] arrObject;
        int indexObject;
        [SerializeField] BaseTargetObject targetObj;
        public bool isEndGame { get; private set; }
        [field: SerializeField] public Transform TranDestination { get; private set; }
        Dictionary<int, List<int>> dicCondition;
        public int ItemCounter { get; set; }
        public int MaxItem { get; private set; }
        [field: SerializeField] public Collider colPanelScore { get; private set; }
        BaseDissolveItem GetItem(Collider col)
        {
            return Extension.GetItemCanInteract(dicDissolveItem, col);
        }
        void Awake()
        {
            PoolingSystem.Preload();
            dicDissolveItem = new();
        }
        void Start()
        {
            indexObject = 0;
            UIManager.Instance.OnInit();
            UIManager.Instance.OpenUI<CanvasGamePlay>();
            SetObject(arrObject[0]);
        }
        void SetObject(BaseTargetObject objectTarget)
        {
            isEndGame = false;
            ItemCounter = 0;
            if (targetObj != null)
            {
                Destroy(targetObj.gameObject);
            }
            dicCondition = new();
            targetObj = PoolingSystem.Spawn(objectTarget);
            targetObj.arrItemDissolve.ForEach(x => dicCondition.Add(x.id, x.childrenIDs.ToList()));
            MaxItem = targetObj.arrItemDissolve.Length;
            EventManager.EmitEvent(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER);
        }
        void Update()
        {
            if (isEndGame) return;
            if (Input.GetMouseButtonDown(0))
            {
                point = Input.mousePosition;
                Ray ray = cameraCtrl.camera.ScreenPointToRay(point);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    BaseDissolveItem item = GetItem(hit.collider);
                    if (item != null)
                    {
                        if (dicCondition[item.id].Count == 0)
                        {
                            foreach (var value in dicCondition.Values)
                            {
                                if (value.Contains(item.id))
                                {
                                    value.Remove(item.id);
                                }
                            }
                            SoundManager.Instance.PlaySound(DataSystem.Instance.gameplaySO.sfxClick);
                            dicCondition.Remove(item.id);
                            item.Dissolve();
                            if (dicCondition.Count == 0)
                            {
                                WinGame();
                            }
                        }
                        else
                        {
                            item.Warning();
                        }
                    }
                }
            }
        }
        public Vector3 GetPoint()
        {
            point = cameraCtrl.camera.ScreenToWorldPoint(Input.mousePosition);
            point.z = 0;
            return point;
        }
        void WinGame()
        {
            isEndGame = true;
            Debug.Log("WinGame");
            StartCoroutine(IEWinGame());
        }
        IEnumerator IEWinGame()
        {
            yield return new WaitUntil(() => ItemCounter == MaxItem);
            yield return new WaitForSeconds(1f);
            UIManager.Instance.OpenUI<CanvasNextGame>();
        }
        public void NextLevel()
        {
            ++indexObject;
            SetObject(arrObject[indexObject]);
        }
    }
}