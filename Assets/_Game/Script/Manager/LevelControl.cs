using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using TrungKien.Core.VFX;
using Sirenix.Utilities;
using UnityEngine.Rendering;
using TrungKien.Core.Gameplay;
using TrungKien.Core.UI;
using System.Xml;

namespace TrungKien.Core
{
    public class LevelControl : Singleton<LevelControl>
    {
        public CameraControl cameraCtrl;
        [SerializeField] Material matPlane;
        Dictionary<Collider, BaseDissolveItem> dicDissolveItem;
        Vector3 point;
        int indexObject;
        [SerializeField] BaseTargetObject targetObj;
        public bool isEndGame { get; private set; }
        public bool isStartGame { get; private set; }
        public int ItemCounter { get; set; }
        public int MaxItem { get; private set; }
        [field: SerializeField] public Collider colPanelScore { get; private set; }
        public float scaleTime = 1;
        public float DetlaTime => Time.deltaTime * scaleTime;
        public float FixDeltaTime => Time.fixedDeltaTime * scaleTime;
        public int currentBowl;
        public List<BowlClass> listBowl;
        public List<BowlCacheClass> listCacheBowl;
        [ShowInInspector] Dictionary<string, List<int>> dicPartColor;
        [ShowInInspector] Dictionary<int, List<string>> dicGenColor;
        [ShowInInspector] Dictionary<int, ColorClass> dicColor;
        const string EVENT_UPDATE_SCORE = "UpdateScore";
        [SerializeField] LayerMask layerObject;
        BaseDissolveItem GetItem(Collider col)
        {
            return Extension.GetItemCanInteract(dicDissolveItem, col);
        }
        public override void Awake()
        {
            base.Awake();
            dicDissolveItem = new();
        }
        void Start()
        {
            indexObject = 0;
            UIManager.Instance.OpenUI<CanvasGamePlay>();
            LevelSO.ModelConfig config = DataSystem.Instance.levelSO.levels[0];
            cameraCtrl.camera.backgroundColor = config.modelSO.colorBG;
            matPlane.color = config.modelSO.colorPlane;
            SetObject(config);
        }

        void SetObject(LevelSO.ModelConfig objectTarget)
        {
            isStartGame = false;
            isEndGame = false;
            dicColor = new();
            dicGenColor = new();
            listCacheBowl = new();
            dicPartColor = new();
            currentBowl = 2;
            ItemCounter = 0;

            SetIDColor(objectTarget);

            int amountGenColor = objectTarget.modelSO.model.arrItemDissolve.Length * (objectTarget.modelSO.layerPerPart - 1);
            Debug.Log($"amountGenColor: {amountGenColor}");

            if (targetObj != null)
            {
                Destroy(targetObj.gameObject);
            }
            targetObj = PoolingSystem.Spawn(objectTarget.modelSO.model);
            targetObj.LoadColor(objectTarget.modelSO.dicColor);
            targetObj.TF.localScale = Vector3.one * targetObj.LocalScale;
            MaxItem = targetObj.arrItemDissolve.Length;

            FillDicGenColor();

            GenColor(objectTarget);
            //EventManager.EmitEvent(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER);

            // control bowl
            listBowl = new();
            List<int> listIDColorBowl = GetBowlIDColor(currentBowl);
            for (int i = 0; i < listIDColorBowl.Count; i++)
            {
                BowlClass bowlClass = new BowlClass()
                {
                    idColor = listIDColorBowl[i],
                    gizmoPos = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.GizmoObj])
                };
                listBowl.Add(bowlClass);
            }
            for (int i = 0; i < DataSystem.Instance.gameplaySO.amountCacheBowl; i++)
            {
                BowlCacheClass cacheBowl = new BowlCacheClass()
                {
                    gizmoPos = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.GizmoObj])
                };
                cacheBowl.Init();
                listCacheBowl.Add(cacheBowl);
            }
            UIManager.Instance.GetUI<CanvasGamePlay>().SetSandBow(listBowl);
            UIManager.Instance.GetUI<CanvasGamePlay>().SetCacheSandBowl(DataSystem.Instance.gameplaySO.amountCacheBowl);
            isStartGame = true;
        }
        public void SetIDColor(LevelSO.ModelConfig objectTarget)
        {
            int indexColor = 0;
            foreach (var item in objectTarget.modelSO.dicColor)
            {
                ColorClass colorClass = new ColorClass()
                {
                    color = item.Key,
                    listPart = new List<string>(item.Value)
                };
                dicColor.Add(indexColor, colorClass);
                dicGenColor.Add(indexColor, new List<string>(item.Value));
                foreach (var part in item.Value)
                {
                    if (!dicPartColor.ContainsKey(part))
                    {
                        dicPartColor.Add(part, new List<int>());
                    }
                    dicPartColor[part].Add(indexColor);
                }
                indexColor++;
            }
        }
        void FillDicGenColor()
        {
            List<string> listPartFillColor = new();
            foreach (var item in dicGenColor)
            {
                while (item.Value.Count % DataSystem.Instance.gameplaySO.maxSandPerBowl != 0)
                {
                    string partName = targetObj.arrItemDissolve[Random.Range(0, targetObj.arrItemDissolve.Length)].itemDissolve.gameObject.name;
                    if (!listPartFillColor.Contains(partName))
                    {
                        listPartFillColor.Add(partName);
                        dicPartColor[partName].Add(item.Key);
                        dicGenColor[item.Key].Add(partName);
                    }
                }
            }
        }
        void GenColor(LevelSO.ModelConfig objectTarget)
        {
            List<string> listPart = new();
            int indexLayer = 1;

            foreach (var item in dicPartColor)
            {
                if (item.Value.Count == indexLayer)
                {
                    listPart.Add(item.Key);
                }
            }
            for (int j = indexLayer; j < objectTarget.modelSO.layerPerPart; j++)
            {
                while (listPart.Count >= DataSystem.Instance.gameplaySO.maxSandPerBowl)
                {
                    int indexColor = GetColorHaveMinPart();
                    for (int i = DataSystem.Instance.gameplaySO.maxSandPerBowl - 1; i >= 0; i--)
                    {
                        dicGenColor[indexColor].Add(listPart[i]);
                        dicPartColor[listPart[i]].Add(indexColor);
                        listPart.RemoveAt(i);
                    }
                }
                List<string> listAllPart = dicPartColor.Keys.ToList();
                listPart.AddRange(listAllPart);
            }
        }
        int GetColorHaveMinPart()
        {
            int result = 0;
            foreach (var item in dicGenColor)
            {
                if (item.Value.Count < dicGenColor[0].Count)
                {
                    result = item.Key;
                }
            }
            return result;
        }
        public List<int> GetBowlIDColor(int amount)
        {
            List<int> totalValidColor = new();
            foreach (var item in dicGenColor)
            {
                if (item.Value.Count >= DataSystem.Instance.gameplaySO.maxSandPerBowl)
                {
                    totalValidColor.Add(item.Key);
                }
            }
            if (amount < totalValidColor.Count)
            {
                List<int> result = new();
                while (result.Count < amount)
                {
                    int idColor = totalValidColor[Random.Range(0, totalValidColor.Count)];
                    if (!result.Contains(idColor))
                    {
                        result.Add(idColor);
                    }
                }
                return result;
            }
            return totalValidColor;
        }

        void Update()
        {
            if (isEndGame || !isStartGame) return;
            if (Input.GetMouseButtonDown(0))
            {
                point = Input.mousePosition;
                Ray ray = cameraCtrl.camera.ScreenPointToRay(point);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerObject))
                {
                    BaseDissolveItem item = GetItem(hit.collider);
                    if (item != null)
                    {
                        item.Dissolve(dicPartColor[item.gameObject.name].Count > 1);
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
            //SetObject(DataSystem.Instance.gameplaySO.arrObject[indexObject]);
        }
        public Color GetColor(int idColor)
        {
            return dicColor[idColor].color;
        }
        public void ItemDissolve(string partName)
        {
            BowlClass bowlClass = listBowl.Find(x => x.idColor == dicPartColor[partName][0]);
            if (bowlClass != null)
            {
                foreach (var item in dicColor)
                {
                    if (item.Value.listPart.Contains(partName))
                    {
                        item.Value.listPart.Remove(partName);
                        break;
                    }
                }
                bowlClass.AddItem();
                dicPartColor[partName].RemoveAt(0);
            }
            else
            {
                BowlCacheClass emptyCacheBowl = GetCacheSandBowl();
                if (emptyCacheBowl != null)
                {
                    emptyCacheBowl.AddItem(dicPartColor[partName][0]);
                    dicPartColor[partName].RemoveAt(0);
                }
            }
            EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_UPDATE_SCORE);
        }
        public Transform GetGizmoPos(string partName)
        {
            BowlClass bowlClass = listBowl.Find(x => x.idColor == dicPartColor[partName][0]);
            if (bowlClass != null)
            {
                return bowlClass.gizmoPos.TF;
            }
            else
            {
                return GetGizmoCachePos();
            }
        }
        Transform GetGizmoCachePos()
        {
            BowlCacheClass cacheBowl = GetCacheSandBowl();
            if (cacheBowl != null)
            {
                return cacheBowl.gizmoPos.TF;
            }
            return null;
        }
        BowlCacheClass GetCacheSandBowl()
        {
            for (int i = 0; i < listCacheBowl.Count; i++)
            {
                if (listCacheBowl[i].isEmpty)
                {
                    return listCacheBowl[i];
                }
            }
            return null;
        }
    }
    [System.Serializable]
    public class BowlClass
    {
        public int counter, idColor;
        public PoolingElement gizmoPos;
        public bool isFull => counter >= DataSystem.Instance.gameplaySO.maxSandPerBowl;
        public void AddItem()
        {
            ++counter;
            if (isFull)
            {
                EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_NEW_BOWL);
                Fix.DelayedCall(0.3f, () =>
                {
                    counter = 0;
                    idColor = LevelControl.Instance.GetBowlIDColor(1)[0];
                });
            }
        }
        public Color GetColor()
        {
            return LevelControl.Instance.GetColor(idColor);
        }
    }
    [System.Serializable]
    public class BowlCacheClass
    {
        public int idColor;
        public PoolingElement gizmoPos;
        public bool isEmpty => idColor == -1;
        public void Init()
        {
            idColor = -1;
        }
        public void AddItem(int idColor)
        {
            this.idColor = idColor;
            // if (index > LevelControl.Instance.maxSandPerBowl)
            // {
            //     index = 0;
            // }
        }
        public Color GetColor()
        {
            return default; // LevelControl.Instance.GetColor(idColor);
        }
    }
    [System.Serializable]
    public class ColorClass
    {
        public Color color;
        public List<string> listPart;
    }
}