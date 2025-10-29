using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrungKien.Core.UI
{
    public class UISandBowl : PoolingElement
    {
        [SerializeField] Image[] imgColor;
        [SerializeField] RectTransform rectRoot;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] GameObject objLock;
        public Button buttonLock;
        public bool isFlyOut { get; private set; }
        [SerializeField] TextMeshProUGUI txtCount;
        Color cacheColor;
        [Sirenix.OdinInspector.ReadOnly][SerializeField] int localCounter;
        public int indexBowl { get; set; }

        public void SetUp(Color color)
        {
            Init();
            imgColor.ForEach(x => x.color = color);
            txtCount.color = color;
            this.cacheColor = color;
        }
        public void Init()
        {
            txtCount.text = "0";
        }
        public void AddSand()
        {
            ++localCounter;
            txtCount.text = localCounter.ExToString();
            DebugCustom.LogColor("Main Bowl Fill", this.cacheColor);
            if (CheckChangeMainBowl(localCounter))
            {
                localCounter = 0;
                LevelControl.Instance.listBowl[indexBowl].idColor = -1;
                StartCoroutine(IEFlyOut());
            }
        }
        IEnumerator IEFlyOut()
        {
            yield return new WaitUntil(() => LevelControl.Instance.amoutSandEffectRunning == 0 && LevelControl.Instance.shareSandCount == 0);
            AnimFlyOut();
        }
        void AnimFlyOut()
        {
            isFlyOut = true;
            canvasGroup.Fade(0f, 1f);
            rectRoot.DOLocalMoveY(200, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                if (LevelControl.Instance.CheckEndLevel())
                {
                    LevelControl.Instance.OnWin();
                }
                else
                {
                    AnimFlyIn();
                }
            });
        }
        void AnimFlyIn()
        {
            LevelControl.Instance.GetNewBowl(indexBowl);
            SetUp(LevelControl.Instance.listBowl[indexBowl].GetColor());
            isFlyOut = false;
            canvasGroup.Fade(1f, 1f);
            rectRoot.DOLocalMoveY(0, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                LevelControl.Instance.GeneralCacheBowlShareMainBowl();
            });
        }
        public void SetLock(bool status)
        {
            objLock.SetActive(status);
        }
        bool CheckChangeMainBowl(int value)
        {
            if (value == DataSystem.Instance.gameplaySO.maxSandPerBowl)
            {
                return true;
            }
            return false;
        }
    }
}