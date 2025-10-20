using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TrungKien.Core.UI
{
    public class CanvasHome : UICanvas
    {
        [SerializeField] Button buttonStartGame;
        public override void SetUp()
        {
            base.SetUp();
            buttonStartGame.onClick.AddListener(() => GameManager.Instance.GoSceneGameplay());
        }

        public override void Open()
        {
            base.Open();
        }
    }
}