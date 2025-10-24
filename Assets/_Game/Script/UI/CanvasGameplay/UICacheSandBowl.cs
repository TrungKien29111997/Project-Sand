using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
namespace TrungKien.Core.UI
{
    public class UICacheSandBowl : PoolingElement
    {
        [SerializeField] Image[] imgColor;
        [SerializeField] Image imgVisualBowl;
        [SerializeField] RectTransform rectRoot;
        [SerializeField] float pourRot;
        public int indexCacheBowl { get; set; }
        public void Fill(Color color)
        {
            DebugCustom.LogColor("Cache Bowl Fill", color);
            imgColor.ForEach(x =>
            {
                x.DOKill();
                x.color = new Color(color.r, color.g, color.b, x.color.a);
                x.DOFade(1f, 2f);
            });
        }
        public void Init()
        {
            Debug.Log("Cache Bowl Init");
            imgColor.ForEach(x =>
            {
                x.DOFade(0f, 2f);
            });
        }
        Color color;
        public void VisualBowlFade(Color color, float alpha)
        {
            this.color = color;
            this.color.a = alpha;
            imgVisualBowl.color = this.color;
        }
        public IEnumerator IEShareSandToPreviousCacheBowl(Vector3 targetPos, System.Action actionFillPour = null)
        {
            rectRoot.DOMove(targetPos + LevelControl.Instance.cameraCtrl.TF.up * 0.15f + LevelControl.Instance.cameraCtrl.TF.right * 0.15f, 0.2f);
            yield return new WaitForSeconds(0.2f);
            rectRoot.DOLocalRotate(Vector3.forward * pourRot, 0.2f);
            actionFillPour?.Invoke();
            yield return new WaitForSeconds(0.6f);
            rectRoot.DOLocalRotate(Vector3.forward, 0.2f);
            rectRoot.DOLocalMove(Vector3.zero, 0.2f);
            yield return new WaitForSeconds(0.2f);
        }
    }
}