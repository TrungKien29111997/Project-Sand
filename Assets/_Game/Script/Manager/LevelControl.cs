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
        [SerializeField] CameraControl cameraCtrl;
        Dictionary<Collider, BaseDissolveItem> dicDessoveItem;
        Vector3 point;
        [SerializeField] BaseTargetObject targetObj;
        bool isEndGame;
        [field: SerializeField] public Transform TranDestination { get; private set; }
        [ShowInInspector] Dictionary<int, List<int>> dicCondition;
        public int ItemCounter => dicCondition.Count;
        public int MaxItem { get; private set; }
        [SerializeField] CanvasGamePlay canvasGamePlay;
        BaseDissolveItem GetItem(Collider col)
        {
            return Extension.GetItemCanInteract(dicDessoveItem, col);
        }
        void Awake()
        {
            dicDessoveItem = new();
            PoolingSystem.Preload();
            dicCondition = new();
        }
        void Start()
        {
            targetObj.arrItemDissolve.ForEach(x => dicCondition.Add(x.id, x.childrenIDs.ToList()));
            MaxItem = targetObj.arrItemDissolve.Length;
            canvasGamePlay.SetCanvas();
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
        }
    }
}