using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;
using TMPro;

namespace TrungKien.Core
{
    public class LoadingPanel : Singleton<LoadingPanel>
    {
        public Image loadingFill;
        public GameObject splashImage;
        public RectTransform loadingObj;
        public TextMeshProUGUI txtLoading;
        string subfix = ".";
        string prefix = "Loading";

        private void Start()
        {
            loadingFill.fillAmount = 0;
        }

        public void VisualTextLoading()
        {
            subfix += ".";
            if (subfix.Length > 3)
                subfix = "";
            txtLoading.text = prefix + subfix;
        }

        public void ShowTextLoading(string text)
        {
            prefix = text;
            VisualTextLoading();
        }

        public void StartLoading()
        {
            splashImage.gameObject.SetActive(true);
            loadingFill.DOKill();
            loadingFill.fillAmount = 0;
            gameObject.SetActive(true);
            loadingFill.DOFillAmount(0.7f, 2f).SetUpdate(UpdateType.Normal, true);
        }

        public IEnumerator EndLoading()
        {
            loadingFill.DOKill();
            bool fading = true;
            loadingFill.DOFillAmount(1, 1f).SetUpdate(UpdateType.Normal, true).OnComplete(() => { fading = false; });
            yield return new WaitUntil(() => !fading);
            loadingObj.gameObject.SetActive(false);
        }
    }
}