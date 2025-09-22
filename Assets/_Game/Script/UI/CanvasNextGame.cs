using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace TrungKien.UI
{
    public class CanvasNextGame : UICanvas
    {
        [SerializeField] Transform tranBoard;
        [SerializeField] Button butNext;
        void Awake()
        {
            butNext.onClick.AddListener(ButtonNext);
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
        void ButtonNext()
        {
            UIManager.Instance.OpenUI<CanvasLoading>().SetCanvas(Loading);
        }
        void Loading()
        {
            LevelControl.Instance.NextLevel();
            Close();
        }
    }
}