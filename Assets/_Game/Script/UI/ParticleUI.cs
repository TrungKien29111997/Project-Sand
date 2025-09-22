using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TrungKien
{
    public class ParticleUI : PoolingElement
    {
        [SerializeField] AnimationCurve animCurveTran, animCurveImage;
        [SerializeField] List<Transform> listTranParticle;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float duration = 1;

        public void Active()
        {
            List<float> randomMultiScale = new();
            listTranParticle.ForEach(r => randomMultiScale.Add(Random.Range(0.1f, 2f)));
            DOTween.To(() => 0f, x =>
            {
                for (int i = 0; i < listTranParticle.Count; i++)
                {
                    listTranParticle[i].gameObject.SetActive(true);
                    listTranParticle[i].localScale = Vector3.one * animCurveTran.Evaluate(x) * randomMultiScale[i];
                }
                canvasGroup.alpha = animCurveImage.Evaluate(x);
            }, 1f, duration).OnComplete(() => listTranParticle.ForEach(r => r.gameObject.SetActive(false)));
        }
    }
}