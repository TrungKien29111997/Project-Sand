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
        public int idColor { get; private set; }
        public bool isFlyOut { get; private set; }
        [SerializeField] TextMeshProUGUI txtCount;

        public void SetUp(int idColor, Color color)
        {
            Init();
            this.idColor = idColor;
            imgColor.ForEach(x => x.color = color);
            txtCount.color = color;
        }
        public void Init()
        {
            txtCount.text = "0";
        }
        public void SetSandLevel(int value, int maxValue)
        {
            txtCount.text = value.ExToString();
            if (value == maxValue)
            {
                AnimFlyOut();
            }
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