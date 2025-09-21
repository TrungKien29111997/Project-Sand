using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace TrungKien.UI
{
    public class CanvasGamePlay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI txtAmount;
        public void SetCanvas()
        {
            EventManager.StartListening(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER, UpdateUIItemCounter);
        }
        void UpdateUIItemCounter()
        {
            txtAmount.text = $"{LevelControl.Instance.ItemCounter}/{LevelControl.Instance.MaxItem}";
        }
    }
}