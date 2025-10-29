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
using UnityEngine.Rendering.Universal;

namespace TrungKien.Core
{
    public class LevelControl : Singleton<LevelControl>
    {
        public CameraControl cameraCtrl;
        [SerializeField] MeshRenderer meshPlane;
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
        public int timeLimit, timeLimitRemain;
        bool isCountTime;
        public bool isFreezeTime { get; private set; }
        [SerializeField] Volume volume;
        private Bloom bloom;
        public bool isBloom { get; private set; }
        [SerializeField] Light mainLight;
        public bool isShadow { get; private set; }
        bool isOpenCanvasSetting;
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
            EventManager.StartListening(Constant.EVENT_GAMEPLAY_OPEN_CANVAS_SETTING, OpenCanvasSetting);
            EventManager.StartListening(Constant.EVENT_GAMEPLAY_CLOSE_CANVAS_SETTING, CloseCanvasSetting);
            EventManager.StartListening(Constant.TIMER_TICK_EVENT, CounterTime);
            // Lấy Bloom từ VolumeProfile
            if (volume.profile.TryGet(out bloom))
            {
                DebugCustom.Log("Found Bloom component!");
            }
            else
            {
                DebugCustom.LogWarning("Bloom not found in VolumeProfile!");
            }

            indexObject = 0;
            UIManager.Instance.OpenUI<CanvasGamePlay>();
            LevelSO.ModelConfig config = DataSystem.Instance.levelSO.levels[0];
            cameraCtrl.mainCam.backgroundColor = config.modelSO.colorBG;

            MaterialPropertyBlock mpbPlane = VFXSystem.GetMPB();
            mpbPlane.SetColor(Constants.pLitBaseColor, config.modelSO.colorPlane);
            meshPlane.SetPropertyBlock(mpbPlane);

            SetObject(config);
        }
        void OpenCanvasSetting()
        {
            isOpenCanvasSetting = true;
        }
        void CloseCanvasSetting()
        {
            isOpenCanvasSetting = false;
        }
        public void FreezeTime()
        {
            isFreezeTime = !isFreezeTime;
        }
        public void Bloom()
        {
            isBloom = !isBloom;
            SetBloomActive(isBloom);
        }
        public void Shadow()
        {
            isShadow = !isShadow;
            mainLight.shadows = isShadow ? LightShadows.Soft : LightShadows.None;
        }

        void SetBloomActive(bool enable)
        {
            if (bloom != null)
                bloom.active = enable; // Bật/tắt Bloom
        }

        void SetObject(LevelSO.ModelConfig objectTarget)
        {
            isCountTime = false;
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
            targetObj.LoadMaterial(DataSystem.Instance.gameplaySO.sandMaterial);
            targetObj.LoadColor(objectTarget.modelSO.dicColor);
            targetObj.TF.localScale = Vector3.one * targetObj.LocalScale;
            cameraCtrl.TF.SetPositionAndRotation(objectTarget.modelSO.camStartPos, Quaternion.Euler(objectTarget.modelSO.camStartRot));

            // Gen them color sao cho chia het cho 3 de dissovle het
            FillDicGenColor();

            // Gen them color cho du cac lop
            GenColor(objectTarget);

            maxScore = GetRemainScore();
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

            // set up time
            timeLimit = objectTarget.modelSO.timeLimit;
            timeLimitRemain = timeLimit;
            uiGameplay.Init();
            uiGameplay.SetUpSandBowl(listBowl);
            uiGameplay.SetUpCacheSandBowl(DataSystem.Instance.gameplaySO.amountCacheBowl);
            targetObj.AnimStartLevel(() =>
            {
                isStartGame = true;
                isCountTime = true;
            });
        }
        void CounterTime()
        {
            if (isCountTime && !isFreezeTime)
            {
                --timeLimitRemain;
                uiGameplay.CounterTime();
                if (timeLimitRemain == 0)
                {
                    if (CheckEndLevel())
                    {
                        OnWin();
                    }
                    else
                    {
                        OnLose();
                    }
                }
            }
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
            List<string> listPartCanAddColor = GetListPartNameCanAddColor();
            foreach (var item in dicGenColor)
            {
                while (item.Value.Count % DataSystem.Instance.gameplaySO.maxSandPerBowl != 0)
                {
                    string partName = listPartCanAddColor.GetRandom();
                    dicPartColor[partName].Add(item.Key);
                    dicGenColor[item.Key].Add(partName);
                    listPartCanAddColor.Remove(partName);
                    if (listPartCanAddColor.Count == 0)
                    {
                        listPartCanAddColor = GetListPartNameCanAddColor();
                    }
                }
            }
        }
        List<string> GetListPartNameCanAddColor()
        {
            List<string> listResult = new();
            targetObj.arrItemDissolve.ForEach(x =>
            {
                if (!x.itemDissolve.IsShell)
                {
                    listResult.Add(x.itemDissolve.gameObject.name);
                }
            });
            return listResult;
        }
        void GenColor(LevelSO.ModelConfig objectTarget)
        {
            List<string> listValidPart = GetListPartNameCanAddColor();
            List<string> listPart = new();
            for (int i = 0; i < listValidPart.Count; i++)
            {
                if (dicPartColor[listValidPart[i]].Count == 1)
                {
                    listPart.Add(listValidPart[i]);
                }
            }
            for (int j = 1; j < objectTarget.modelSO.layerPerPart; j++)
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
                List<string> listAllPart = GetListPartNameCanAddColor();
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
            if (!isOpenCanvasSetting && !isLockInteract && Input.GetMouseButtonDown(0))
            {
                point = Input.mousePosition;
                Ray ray = cameraCtrl.mainCam.ScreenPointToRay(point);
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
            point = cameraCtrl.mainCam.ScreenToWorldPoint(Input.mousePosition);
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
            UIManager.Instance.OpenUI<CanvasNextGame>();
        }
        void OnLose()
        {
            isCountTime = false;
            isEndGame = true;
            Debug.Log("Lose");
            UIManager.Instance.OpenUI<CanvasLoseGame>();
        }
        public void ResetLevel()
        {
            SetObject(DataSystem.Instance.levelSO.levels[indexObject]);
        }
        public void NextLevel()
        {
            ++indexObject;
            SetObject(DataSystem.Instance.levelSO.levels[indexObject]);
        }
        public Sprite GetCurrentObejctShadow()
        {
            return DataSystem.Instance.levelSO.levels[indexObject].modelSO.shadow;
        }
        public Sprite GetNextObejctShadow()
        {
            return DataSystem.Instance.levelSO.levels[indexObject + 1].modelSO.shadow;
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
                        uICacheSandBowl.Fill(cacheColor);
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
        public int shareSandCount { get; private set; } = 0;
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
                        shareSandCount++;
                        uiGameplay.VisualShareSandCacheBowlToMainBowl(mainBowlIndex, cacheBowlIndex, item.Value.indexColor, 1.5f);
                        Fix.DelayedCall(1.5f, () => shareSandCount--);
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
            bool hasMiddleNegative = HasMiddleNegative(listIndexColor);
            DebugCustom.Log($"Rong {hasMiddleNegative}");
            if (dicEffectChangeBowl.Keys.Count <= 0 && hasMiddleNegative)
            {
                isLockInteract = true;
                yield return new WaitForSeconds(1.2f);
                List<int> listSortCacheBowl = SortCacheBowl();
                yield return uiGameplay.IEVisualShareBowl(listSortCacheBowl);
            }
        }
        bool HasMiddleNegative(List<int> list)
        {
            int indexOfNegative = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == -1)
                {
                    indexOfNegative = i;
                    break;
                }
            }
            if (indexOfNegative == -1)
            {
                return false;
            }
            else
            {
                for (int i = indexOfNegative; i < list.Count; i++)
                {
                    if (list[i] != -1)
                    {
                        return true;
                    }
                }
                return false;
            }
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
        public void AddMainBowl()
        {
            ++currentBowl;
            BowlClass bowlClass = new BowlClass();
            bowlClass.counter = 0;
            List<int> listValidColor = GetListValidColor();
            List<int> listAllColor = GetListRemainColor();
            bowlClass.idColor = listValidColor.Count > 0 ? listValidColor.GetRandom() : listAllColor.GetRandom();
            listBowl.Add(bowlClass);
            uiGameplay.UpdateMainSandBowl(currentBowl - 1);
        }
        public void AddCacheBowl()
        {
            BowlCacheClass cacheBowl = new BowlCacheClass();
            cacheBowl.Init();
            listCacheBowl.Add(cacheBowl);
            uiGameplay.AddCacheSandBowl();
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