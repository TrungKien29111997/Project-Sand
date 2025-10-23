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

        public void Fill(Color color)
        {
            DebugCustom.LogColor("Cache Bowl Fill", color);
            imgColor.ForEach(x =>
            {
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
    }
}