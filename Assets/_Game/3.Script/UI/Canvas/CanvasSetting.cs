using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TrungKien.Core.UI
{
    public class CanvasSetting : UICanvas
    {
        [SerializeField] Button butClose, butNextLevel;
        [SerializeField] UIOnOff uiSetBloom, uiSetShadow, uiCheat, uiSetTimeCounter;
        void Start()
        {
            uiSetBloom.button.SetButton(SettingBloom);
            uiSetShadow.button.SetButton(SettingShadow);
            uiCheat.button.SetButton(SettingCheat);
            uiSetTimeCounter.button.SetButton(SettingTimeCounter);
            butClose.SetButton(Close);
            butNextLevel.SetButton(ButtonNextLevel);
        }
        public override void SetUp()
        {
            base.SetUp();
            EventManager.StartListening(Constant.EVENT_CHEAT_MODE, CheatShow);
        }
        public override void Open()
        {
            base.Open();
            EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_OPEN_CANVAS_SETTING);
        }
        public override void Close()
        {
            base.Close();
            EventManager.StopListening(Constant.EVENT_CHEAT_MODE, CheatShow);
            EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_CLOSE_CANVAS_SETTING);
        }
        void ButtonNextLevel()
        {
            UIManager.Instance.OpenUI<CanvasLoading>().SetCanvas(LevelControl.Instance.GetNextObejctShadow(), LevelControl.Instance.NextLevel);
            Close();
        }

        void SettingBloom()
        {
            LevelControl.Instance.Bloom();
            uiSetBloom.SetOnOff(LevelControl.Instance.isBloom);
        }
        void SettingShadow()
        {
            LevelControl.Instance.Shadow();
            uiSetShadow.SetOnOff(LevelControl.Instance.isShadow);
        }
        void SettingCheat()
        {
            GameManager.Instance.ShowCheat();
            uiCheat.SetOnOff(GameManager.Instance.IsCheat);
        }
        void SettingTimeCounter()
        {
            LevelControl.Instance.FreezeTime();
            uiSetTimeCounter.SetOnOff(LevelControl.Instance.isFreezeTime);
        }
        void CheatShow()
        {
            uiSetTimeCounter.gameObject.SetActive(GameManager.Instance.IsCheat);
            butNextLevel.gameObject.SetActive(GameManager.Instance.IsCheat);
        }
    }
}