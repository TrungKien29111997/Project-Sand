using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TrungKien.UI
{
    public class UISandBowl : PoolingElement
    {
        [SerializeField] Image[] imgColor;
        [SerializeField] RectTransform rectSandLevel, rectRoot;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] GameObject objLock;
        public Button buttonLock;
        const int defaultHeight = -240;
        public int idColor { get; private set; }
        public bool isFlyOut { get; private set; }

        public void SetUp(int idColor, Color color)
        {
            Init();
            this.idColor = idColor;
            imgColor.ForEach(x => x.color = color);
        }
        public void Init()
        {
            rectSandLevel.localPosition = Vector3.up * defaultHeight;
        }
        public void SetSandLevel(int value, int maxValue)
        {
            rectSandLevel.DOLocalMove(Vector3.up * defaultHeight * ((maxValue - value) / (float)maxValue), 0.5f).OnComplete(() =>
            {
                if (value == maxValue)
                {
                    AnimFlyOut();
                }
            });
        }
        public void AnimFlyOut(System.Action doneAction = null)
        {
            isFlyOut = true;
            canvasGroup.Fade(0f, 1f);
            rectRoot.DOLocalMoveY(200, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                AnimFlyIn();
                doneAction?.Invoke();
            });
        }
        public void AnimFlyIn()
        {
            isFlyOut = false;
            canvasGroup.Fade(1f, 1f);
            rectRoot.DOLocalMoveY(0, 1f).SetEase(Ease.Linear);
        }
        public void SetLock(bool status)
        {
            objLock.SetActive(status);
        }
    }
}