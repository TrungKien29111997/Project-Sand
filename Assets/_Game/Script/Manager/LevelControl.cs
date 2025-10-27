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
        [SerializeField] LayerMask layerObject;
        CanvasGamePlay uiGameplay => UIManager.Instance.GetUI<CanvasGamePlay>();
        public bool isLockInteract;
        public int maxScore { get; private set; }
        public int currentScore => maxScore - GetRemainScore();
        public int GetRemainScore()
        {
            int result = 0;
            foreach (var item in dicGenColor)
            {
                result += item.Value.Count;
            }
            return result;
        }

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

            SetIDColor(objectTarget);

            int amountGenColor = objectTarget.modelSO.model.arrItemDissolve.Length * (objectTarget.modelSO.layerPerPart - 1);
            DebugCustom.Log($"amountGenColor: {amountGenColor}");

            if (targetObj != null)
            {
                Destroy(targetObj.gameObject);
            }
            targetObj = PoolingSystem.Spawn(objectTarget.modelSO.model);
            targetObj.LoadColor(objectTarget.modelSO.dicColor);
            targetObj.TF.localScale = Vector3.one * targetObj.LocalScale;

            FillDicGenColor();

            GenColor(objectTarget);
            maxScore = objectTarget.modelSO.model.arrItemDissolve.Length * objectTarget.modelSO.layerPerPart;
            EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_UPDATE_SCORE);

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
                if (item.Value.Count > 0)
                {
                    if (!dicCaculateColor.ContainsKey(item.Value[0]))
                    {
                        dicCaculateColor.Add(item.Value[0], new());
                    }
                    dicCaculateColor[item.Value[0]]++;
                }
            }
            for (int i = 0; i < listCacheBowl.Count; i++)
            {
                int index = i;
                if (listCacheBowl[index].idColor != -1)
                {
                    if (!dicCaculateColor.ContainsKey(listCacheBowl[index].idColor))
                    {
                        dicCaculateColor.Add(listCacheBowl[index].idColor, new());
                    }
                    dicCaculateColor[listCacheBowl[index].idColor]++;
                }
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
            List<int> listCurrentBowlColor = new();
            for (int i = 0; i < listBowl.Count; i++)
            {
                if (listBowl[i] != null)
                {
                    listCurrentBowlColor.Add(listBowl[i].idColor);
                }
            }
            for (int i = listValidBowl.Count - 1; i >= 0; i--)
            {
                int index = i;
                if (listCurrentBowlColor.Contains(listValidBowl[index]))
                {
                    listValidBowl.RemoveAt(index);
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
            if (!isLockInteract && Input.GetMouseButtonDown(0))
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
        public bool CheckEndLevel()
        {
            return dicGenColor.Values.All(x => x.Count == 0) && listCacheBowl.All(x => x.isEmpty);
        }
        public void OnWin()
        {
            isEndGame = true;
            Debug.Log("WinGame");
        }
        void OnLose()
        {

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
                if (item.Value.Count > 0 && listBowl.Exists(x => x.idColor == item.Value[0]))
                {
                    return true;
                }
            }
            return false;
        }
        public int amoutSandEffectRunning { get; set; } = 0;
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
                        Fix.DelayedCall(DataSystem.Instance.gameplaySO.timeSandFlyOut * 0.3f, () =>
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
            dicGenColor[dicPartColor[partName][0]].Remove(partName);
            dicPartColor[partName].RemoveAt(0);

            if (dicPartColor[partName].Count > 0)
            {
                return dicConfigColor[dicPartColor[partName][0]];
            }
            else
            {
                return default;
            }
        }
        public void GetNewBowl(int indexBowl)
        {
            BowlClass bowlClass = listBowl[indexBowl];
            bowlClass.counter = 0;
            List<int> listValidColor = GetListValidColor();
            List<int> listAllColor = GetListRemainColor();
            bowlClass.idColor = listValidColor.Count > 0 ? listValidColor.GetRandom() : listAllColor.GetRandom();
        }
        public Dictionary<int, ChangeSandBowl> dicEffectChangeBowl;
        public void GeneralCacheBowlShareMainBowl()
        {
            dicEffectChangeBowl = new();
            for (int i = 0; i < listCacheBowl.Count; i++)
            {
                int index = i;
                BowlCacheClass item = listCacheBowl[index];
                if (item.idColor != -1)
                {
                    int indexMainBowl = listBowl.FindIndex(x => x.idColor == item.idColor);
                    if (indexMainBowl != -1 && !listBowl[indexMainBowl].isFull)
                    {
                        if (!dicEffectChangeBowl.ContainsKey(indexMainBowl))
                        {
                            dicEffectChangeBowl.Add(indexMainBowl, new ChangeSandBowl() { indexColor = item.idColor, listIndexCacheBowl = new() });
                        }
                        dicEffectChangeBowl[indexMainBowl].listIndexCacheBowl.Add(index);
                    }
                }
            }
            StartCoroutine(IECacheBowlShareMainBowl());
        }
        IEnumerator IECacheBowlShareMainBowl()
        {
            if (dicEffectChangeBowl.Keys.Count > 0)
            {
                List<int> listEmptyKey = new();
                foreach (var item in dicEffectChangeBowl)
                {
                    int lastIndex = (item.Value.listIndexCacheBowl.Count >= DataSystem.Instance.gameplaySO.maxSandPerBowl) ? (item.Value.listIndexCacheBowl.Count - DataSystem.Instance.gameplaySO.maxSandPerBowl) : 0;
                    for (int i = item.Value.listIndexCacheBowl.Count - 1; i >= lastIndex; i--)
                    {
                        int mainBowlIndex = item.Key;
                        int cacheBowlIndex = item.Value.listIndexCacheBowl[i];
                        uiGameplay.VisualShareSandCacheBowlToMainBowl(mainBowlIndex, cacheBowlIndex, item.Value.indexColor);
                        listCacheBowl[cacheBowlIndex].Init();
                        listBowl[mainBowlIndex].AddSand();
                        item.Value.listIndexCacheBowl.RemoveAt(i);
                    }
                    if (item.Value.listIndexCacheBowl.Count == 0)
                    {
                        listEmptyKey.Add(item.Key);
                    }
                }
                foreach (var item in listEmptyKey)
                {
                    dicEffectChangeBowl.Remove(item);
                }
            }
            List<int> listIndexColor = new();
            for (int i = 0; i < listCacheBowl.Count; i++)
            {
                listIndexColor.Add(listCacheBowl[i].idColor);
            }
            if (dicEffectChangeBowl.Keys.Count <= 0 && HasMiddleNegative(listIndexColor))
            {
                isLockInteract = true;
                yield return new WaitForSeconds(2.2f);
                List<int> listSortCacheBowl = SortCacheBowl();
                yield return uiGameplay.IEVisualShareBowl(listSortCacheBowl);
            }
        }
        bool HasMiddleNegative(List<int> list)
        {
            if (list == null || list.Count == 0)
                return false;

            // Tìm vị trí phần tử âm cuối cùng
            int lastNegativeIndex = -1;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] < 0)
                {
                    lastNegativeIndex = i;
                    break;
                }
            }

            // Nếu không có phần tử âm nào → false
            if (lastNegativeIndex == -1)
                return false;

            // Kiểm tra xem trước đó có phần tử âm nào không
            for (int i = 0; i < lastNegativeIndex; i++)
            {
                if (list[i] < 0)
                    return true; // Có âm nằm trước âm cuối → "âm giữa"
            }

            return false; // Chỉ có chuỗi âm ở cuối
        }

        List<int> SortCacheBowl()
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
#if UNITY_EDITOR
        [SerializeField] bool testColor;
        [SerializeField] int targetColor;
#endif
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
        public int indexColor;
        public List<int> listIndexCacheBowl;
    }
}