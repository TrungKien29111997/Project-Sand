using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using TrungKien.UI;
using TrungKien.Core.VFX;
using Sirenix.Utilities;
using UnityEngine.Rendering;
namespace TrungKien
{
    public class LevelControl : Singleton<LevelControl>
    {
        public CameraControl cameraCtrl;
        Dictionary<Collider, BaseDissolveItem> dicDissolveItem;
        Vector3 point;
        int indexObject;
        [SerializeField] BaseTargetObject targetObj;
        public bool isEndGame { get; private set; }
        public bool isStartGame { get; private set; }
        //Dictionary<int, List<int>> dicCondition;
        public int ItemCounter { get; set; }
        public int MaxItem { get; private set; }
        [field: SerializeField] public Collider colPanelScore { get; private set; }
        public float scaleTime = 1;
        public float DetlaTime => Time.deltaTime * scaleTime;
        public float FixDeltaTime => Time.fixedDeltaTime * scaleTime;
        public int maxSandPerBowl { get; private set; } = 3;
        public int maxColor = 3, currentBowl;
        public List<BowlClass> listBowl;
        public List<BowlCacheClass> listCacheBowl;
        [ShowInInspector] Dictionary<int, List<int>> dicGenColor;
        [ShowInInspector] Dictionary<int, ColorClass> dicColor;
        [SerializeField] int layerFactor = 5;
        public int amountCacheBowl = 5;
        const string EVENT_UPDATE_SCORE = "UpdateScore";
        BaseDissolveItem GetItem(Collider col)
        {
            return Extension.GetItemCanInteract(dicDissolveItem, col);
        }
        void Awake()
        {
            PoolingSystem.Init();
            VFXSystem.Init();
            dicDissolveItem = new();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 1000;
            bool enabled = UnityEngine.Rendering.GraphicsSettings.useScriptableRenderPipelineBatching;
            Debug.Log("🔍 SRP Batcher: " + (enabled ? "ENABLED ✅" : "DISABLED ❌"));
        }
        void Start()
        {
            Debug.Log(GraphicsSettings.useScriptableRenderPipelineBatching);
            indexObject = 0;
            UIManager.Instance.OnInit();
            UIManager.Instance.OpenUI<CanvasGamePlay>();
            SetObject(DataSystem.Instance.gameplaySO.arrObject[0]);
        }
        [ShowInInspector] Dictionary<int, List<int>> dicRandomColor;

        void SetObject(BaseTargetObject objectTarget)
        {
            isStartGame = false;
            dicColor = new();
            dicGenColor = new();
            listCacheBowl = new();
            List<Material> listMaterial = new();
            objectTarget.arrItemDissolve.ForEach(x =>
            {
                Material mat = x.itemDissolve.GetShareMaterial();
                if (!listMaterial.Contains(mat))
                {
                    listMaterial.Add(mat);
                }
                dicGenColor.Add(x.id, new());
                int indexMat = listMaterial.IndexOf(mat);
                dicGenColor[x.id].Add(indexMat);

                ColorClass colorClass = new ColorClass()
                {
                    color = mat.GetColor(Constants.pShaderSandColor),
                    mat = mat,
                    listPart = new()
                };
                if (!dicColor.ContainsKey(indexMat))
                {
                    dicColor.Add(indexMat, colorClass);
                }
                dicColor[indexMat].listPart.Add(x.id);
            });
            int maxLayerPerColor = maxColor * layerFactor * objectTarget.arrItemDissolve.Length;

            // gen color
            dicRandomColor = new();
            foreach (var item in dicColor)
            {
                item.Value.colorCounter = maxLayerPerColor - item.Value.listPart.Count;
                int partAmount = item.Value.colorCounter / objectTarget.arrItemDissolve.Length;
                for (int i = 0; i < objectTarget.arrItemDissolve.Length; i++)
                {
                    int partID = objectTarget.arrItemDissolve[i].id;
                    for (int j = 0; j < partAmount; j++)
                    {
                        item.Value.listPart.Add(partID);
                        if (!dicRandomColor.ContainsKey(partID))
                        {
                            dicRandomColor.Add(partID, new());
                        }
                        dicRandomColor[partID].Add(item.Key);
                    }
                }
                for (int i = 0; i < item.Value.colorCounter % objectTarget.arrItemDissolve.Length; i++)
                {
                    int partID = objectTarget.arrItemDissolve[Random.Range(0, objectTarget.arrItemDissolve.Length)].id;
                    item.Value.listPart.Add(partID);
                    dicRandomColor[partID].Add(item.Key);
                }
            }
            foreach (var item in dicGenColor)
            {
                List<int> listIdColor = dicRandomColor[item.Key];
                while (listIdColor.Count > 0)
                {
                    int index = Random.Range(0, listIdColor.Count);
                    item.Value.Add(listIdColor[index]);
                    listIdColor.RemoveAt(index);
                }
            }
            //dicRandomColor.Clear();

            currentBowl = 2;
            isEndGame = false;
            ItemCounter = 0;
            if (targetObj != null)
            {
                Destroy(targetObj.gameObject);
            }
            //dicCondition = new();
            targetObj = PoolingSystem.Spawn(objectTarget);
            targetObj.TF.localScale = Vector3.one * targetObj.LocalScale;
            //targetObj.arrItemDissolve.ForEach(x => dicCondition.Add(x.id, x.childrenIDs.ToList()));
            MaxItem = targetObj.arrItemDissolve.Length;
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
            for (int i = 0; i < amountCacheBowl; i++)
            {
                BowlCacheClass cacheBowl = new BowlCacheClass()
                {
                    gizmoPos = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.GizmoObj])
                };
                cacheBowl.Init();
                listCacheBowl.Add(cacheBowl);
            }
            UIManager.Instance.GetUI<CanvasGamePlay>().SetSandBow(listBowl);
            UIManager.Instance.GetUI<CanvasGamePlay>().SetCacheSandBowl(amountCacheBowl);
            isStartGame = true;
        }
        public List<int> GetBowlIDColor(int amount)
        {
            List<int> totalValidColor = new();
            foreach (var item in dicColor)
            {
                List<int> idPart = new();
                foreach (var id in item.Value.listPart)
                {
                    if (!idPart.Contains(id))
                    {
                        idPart.Add(item.Key);
                        if (idPart.Count >= maxColor)
                        {
                            totalValidColor.Add(item.Key);
                            break;
                        }
                    }
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
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    BaseDissolveItem item = GetItem(hit.collider);
                    if (item != null)
                    {
                        if (dicGenColor[item.id].Count > 1)
                        {
                            Material mat = dicColor[dicGenColor[item.id][1]].mat;
                            item.Dissolve(mat, false);
                        }
                        else
                        {
                            item.Dissolve(null, true);
                        }
                    }
                    // if (item != null)
                    // {
                    //     if (dicCondition[item.id].Count == 0)
                    //     {
                    //         foreach (var value in dicCondition.Values)
                    //         {
                    //             if (value.Contains(item.id))
                    //             {
                    //                 value.Remove(item.id);
                    //             }
                    //         }
                    //         SoundManager.Instance.PlaySound(DataSystem.Instance.gameplaySO.sfxClick);
                    //         dicCondition.Remove(item.id);
                    //         item.Dissolve();
                    //         if (dicCondition.Count == 0)
                    //         {
                    //             WinGame();
                    //         }
                    //     }
                    //     else
                    //     {
                    //         item.Warning();
                    //     }
                    // }
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
            SetObject(DataSystem.Instance.gameplaySO.arrObject[indexObject]);
        }
        public Color GetColor(int idColor)
        {
            return dicColor[idColor].color;
        }
        public void ItemDissolve(int partID)
        {
            BowlClass bowlClass = listBowl.Find(x => x.idColor == dicGenColor[partID][0]);
            if (bowlClass != null)
            {
                foreach (var item in dicColor)
                {
                    if (item.Value.listPart.Contains(partID))
                    {
                        item.Value.listPart.Remove(partID);
                        break;
                    }
                }
                bowlClass.AddItem();
                dicGenColor[partID].RemoveAt(0);
            }
            else
            {
                BowlCacheClass emptyCacheBowl = GetCacheSandBowl();
                if (emptyCacheBowl != null)
                {
                    emptyCacheBowl.AddItem(dicGenColor[partID][0]);
                    dicGenColor[partID].RemoveAt(0);
                }
            }
            EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_UPDATE_SCORE);
        }
        public Transform GetGizmoPos(int partID)
        {
            BowlClass bowlClass = listBowl.Find(x => x.idColor == dicGenColor[partID][0]);
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
        public bool isFull => counter >= LevelControl.Instance.maxSandPerBowl;
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
            return LevelControl.Instance.GetColor(idColor);
        }
    }
    [System.Serializable]
    public class ColorClass
    {
        public int colorCounter;
        public Color color;
        public Material mat;
        public List<int> listPart;
    }
}