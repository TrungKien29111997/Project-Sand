using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
namespace TrungKien.Core.UI
{
    public class CanvasLoseGame : UICanvas
    {
        [SerializeField] Transform tranBoard;
        [SerializeField] Button butAgain, butHome;
        void Awake()
        {
            butAgain.onClick.AddListener(ButtonAgain);
            butHome.onClick.AddListener(ButtonHome);
        }
        public override void SetUp()
        {
            base.SetUp();
            tranBoard.localScale = Vector3.zero;
        }
        public override void Open()
        {
            base.Open();
            tranBoard.DOScale(1.15f, 0.4f).OnComplete(() => tranBoard.DOScale(1f, 0.2f));
        }
        void ButtonAgain()
        {
            UIManager.Instance.OpenUI<CanvasLoading>().SetCanvas(LevelControl.Instance.GetCurrentObejctShadow(), Loading);
            Close();
        }
        void ButtonHome()
        {
            Close();
            GameManager.Instance.GoSceneHome();
        }
        void Loading()
        {
            LevelControl.Instance.ResetLevel();
        }
    }
}