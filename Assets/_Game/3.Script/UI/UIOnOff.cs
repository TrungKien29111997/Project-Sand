using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TrungKien.Core.UI
{
    public class UIOnOff : MonoBehaviour
    {
        public Button button;
        [SerializeField] Image imgOn, imgOff;
        public void SetOnOff(bool isOn)
        {
            if (imgOn != null) imgOn.gameObject.SetActive(isOn);
            if (imgOff != null) imgOff.gameObject.SetActive(!isOn);
        }
    }
}