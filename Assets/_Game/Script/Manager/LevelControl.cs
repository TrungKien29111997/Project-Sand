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
using Unity.VisualScripting.FullSerializer;

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
        [ShowInInspector] Dictionary<int, Color> dicConfigColor;
        const string EVENT_UPDATE_SCORE = "UpdateScore";
        [SerializeField] LayerMask layerObject;
        CanvasGamePlay uiGameplay => UIManager.Instance.GetUI<CanvasGamePlay>();
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
            dicConfigColor = new();
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
            List<int> listValidColor = GetListValidColor();
            List<int> listAllColor = GetListRemainColor();
            for (int i = 0; i < currentBowl; i++)
            {
                BowlClass bowlClass = new BowlClass()
                {
                    idColor = i < listValidColor.Count ? listValidColor[i] : listAllColor.GetRandom(),
                };
                listBowl.Add(bowlClass);
            }
            for (int i = 0; i < DataSystem.Instance.gameplaySO.amountCacheBowl; i++)
            {
                BowlCacheClass cacheBowl = new BowlCacheClass();
                cacheBowl.Init();
                listCacheBowl.Add(cacheBowl);
            }
            uiGameplay.SetUpSandBowl(listBowl);
            uiGameplay.SetUpCacheSandBowl(DataSystem.Instance.gameplaySO.amountCacheBowl);
            isStartGame = true;
        }
        public void SetIDColor(LevelSO.ModelConfig objectTarget)
        {
            foreach (var item in objectTarget.modelSO.dicColor)
            {
                dicConfigColor.Add(item.Key, item.Value.color);
                dicGenColor.Add(item.Key, new List<string>(item.Value.listPart));
                foreach (var part in item.Value.listPart)
                {
                    if (!dicPartColor.ContainsKey(part))
                    {
                        dicPartColor.Add(part, new List<int>());
                    }
                    dicPartColor[part].Add(item.Key);
                }
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

        List<int> GetListValidColor()
        {
            Dictionary<int, int> dicCaculateColor;
            List<int> listValidBowl = new();
            dicCaculateColor = new();
            foreach (var item in dicPartColor)
            {
                if (!dicCaculateColor.ContainsKey(item.Value[0]))
                {
                    dicCaculateColor.Add(item.Value[0], new());
                }
                dicCaculateColor[item.Value[0]]++;
            }
            foreach (var item in dicCaculateColor)
            {
                int amountValidBowl = item.Value / DataSystem.Instance.gameplaySO.maxSandPerBowl;
                if (amountValidBowl > 0)
                {
                    for (int i = 0; i < amountValidBowl; i++)
                    {
                        listValidBowl.Add(item.Key);
                    }
                }
            }
            if (!listBowl.IsNullOrEmpty())
            {
                for (int i = listValidBowl.Count - 1; i >= 0; i--)
                {
                    if (listBowl.Exists(x => x.idColor == listValidBowl[i]))
                    {
                        listValidBowl.RemoveAt(i);
                    }
                }
            }
            listValidBowl.Shuffle();
            return listValidBowl;
        }
        List<int> GetListRemainColor()
        {
            List<int> result = new();
            foreach (var item in dicGenColor)
            {
                if (item.Value.Count >= DataSystem.Instance.gameplaySO.maxSandPerBowl)
                {
                    result.Add(item.Key);
                }
            }
            return result;
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
                        if (CheckCanDissolve(item.gameObject.name))
                        {
                            item.Dissolve(dicPartColor[item.gameObject.name].Count <= 1);
                        }
                        else
                        {
                            if (CheckCanAnyDissove())
                            {
                                uiGameplay.WarningFullCacheBowl();
                            }
                            else
                            {
                                OnLose();
                            }
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
        void OnWin()
        {
            isEndGame = true;
            Debug.Log("WinGame");
            StartCoroutine(IEWinGame());
        }
        void OnLose()
        {

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
            if (dicConfigColor.ContainsKey(idColor))
            {
                return dicConfigColor[idColor];
            }
            return Color.clear;
        }
        // public Transform GetGizmoPos(string partName)
        // {
        //     BowlClass bowlClass = listBowl.Find(x => x.idColor == dicPartColor[partName][0]);
        //     if (bowlClass != null)
        //     {
        //         return bowlClass.gizmoPos.TF;
        //     }
        //     else
        //     {
        //         return GetGizmoCachePos();
        //     }
        // }
        // Transform GetGizmoCachePos()
        // {
        //     BowlCacheClass cacheBowl = listCacheBowl.Find(x => x.isEmpty);
        //     if (cacheBowl != null)
        //     {
        //         return cacheBowl.gizmoPos.TF;
        //     }
        //     return null;
        // }
        bool CheckCanDissolve(string partName)
        {
            int index = listBowl.FindIndex(x => x.idColor == dicPartColor[partName][0] && !x.isFull);
            if (index != -1)
            {
                return true;
            }
            else
            {
                int indexCache = listCacheBowl.FindIndex(x => x.isEmpty);
                if (indexCache != -1)
                {
                    return true;
                }
            }
            return false;
        }
        bool CheckCanAnyDissove()
        {
            foreach (var item in dicPartColor)
            {
                if (listBowl.Exists(x => x.idColor == item.Value[0]))
                {
                    return true;
                }
            }
            return false;
        }
        public System.Tuple<Transform, System.Action> PreFillBowl(string partName)
        {
            int index = listBowl.FindIndex(x => x.idColor == dicPartColor[partName][0] && !x.isFull);
            if (index != -1)
            {
                listBowl[index].AddSand();
                UISandBowl uiBowl = uiGameplay.listUISandBowl[index];
                return new System.Tuple<Transform, System.Action>(uiGameplay.listUISandBowl[index].transform, () =>
                {
                    uiBowl.AddSand();
                });
            }
            else
            {
                int indexCache = listCacheBowl.FindIndex(x => x.isEmpty);
                if (indexCache != -1)
                {
                    listCacheBowl[indexCache].idColor = dicPartColor[partName][0];
                    Color cacheColor = dicConfigColor[dicPartColor[partName][0]];
                    UICacheSandBowl uICacheSandBowl = uiGameplay.listUICacheSandBowl[indexCache];
                    return new System.Tuple<Transform, System.Action>(uiGameplay.listUICacheSandBowl[indexCache].transform, () =>
                    {
                        Fix.DelayedCall(0.8f, () =>
                            {
                                uICacheSandBowl.Fill(cacheColor);
                            });
                    });
                }
            }
            return null;
        }
        public Color GetNewColorAndClearOldColor(string partName)
        {
            dicPartColor[partName].RemoveAt(0);
            dicGenColor[dicPartColor[partName][0]].Remove(partName);
            return dicConfigColor[dicPartColor[partName][0]];
        }
        public void GetNewBowl(int indexBowl)
        {
            BowlClass bowlClass = listBowl[indexBowl];
            bowlClass.counter = 0;
            List<int> listValidColor = GetListValidColor();
            List<int> listAllColor = GetListRemainColor();
            bowlClass.idColor = listValidColor.Count > 0 ? listValidColor.GetRandom() : listAllColor.GetRandom();
        }
        public List<ChangeSandBowl> listEffectChangeBowl;
        public void GeneralCacheBowlShareMainBowl()
        {
            listEffectChangeBowl = new();
            for (int i = 0; i < listCacheBowl.Count; i++)
            {
                BowlCacheClass item = listCacheBowl[i];
                if (item.idColor != -1)
                {
                    int indexMainBowl = listBowl.FindIndex(x => x.idColor == item.idColor);
                    if (indexMainBowl != -1 && !listBowl[indexMainBowl].isFull)
                    {
                        listBowl[indexMainBowl].AddSand();
                        listEffectChangeBowl.Add(new ChangeSandBowl()
                        {
                            indexColor = item.idColor,
                            indexCacheBowl = i,
                            indexMainBowl = indexMainBowl,
                        });
                        item.Init();
                    }
                }
            }
        }
        public List<int> SortCacheBowl()
        {
            List<int> listEmptyBowl = new();
            List<BowlCacheClass> sortListCacheBowl = new();
            for (int i = 0; i < listCacheBowl.Count; i++)
            {
                if (!listCacheBowl[i].isEmpty)
                {
                    sortListCacheBowl.Add(listCacheBowl[i]);
                    listEmptyBowl.Add(1);
                }
                else
                {
                    listEmptyBowl.Add(0);
                }
            }
            int amountCacheBowlEmpty = listCacheBowl.Count - sortListCacheBowl.Count;
            for (int i = 0; i < amountCacheBowlEmpty; i++)
            {
                sortListCacheBowl.Add(new BowlCacheClass() { idColor = -1 });
            }
            listCacheBowl = sortListCacheBowl;
            return listEmptyBowl;
        }
    }
    [System.Serializable]
    public class BowlClass
    {
        public int counter, idColor;
        public bool isFull => counter >= DataSystem.Instance.gameplaySO.maxSandPerBowl;
        public void AddSand()
        {
            ++counter;
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
        public bool isEmpty => idColor == -1;
        public void Init()
        {
            idColor = -1;
        }
        public Color GetColor()
        {
            return LevelControl.Instance.GetColor(idColor);
        }
    }
    [System.Serializable]
    public class ChangeSandBowl
    {
        public int indexColor, indexCacheBowl, indexMainBowl;
    }
}