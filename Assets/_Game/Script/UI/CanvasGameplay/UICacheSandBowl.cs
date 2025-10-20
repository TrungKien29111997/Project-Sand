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
        public int idColor { get; private set; }
        public bool isEmpty => idColor == -1;

        public void AddItem(int idColor, Color color)
        {
            this.idColor = idColor;
            imgColor.ForEach(x =>
            {
                x.color = color;
                x.DOFade(1, 1.5f);
            });
        }
        public int RemoveItem()
        {
            Init();
            return idColor;
        }
        public void Init()
        {
            idColor = -1;
            imgColor.ForEach(x => x.DOFade(0, 0.2f));
        }
    }
}