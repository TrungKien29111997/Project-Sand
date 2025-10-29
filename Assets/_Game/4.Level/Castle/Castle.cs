using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace TrungKien.Core.Gameplay
{
    public class Castle : BaseTargetObject
    {
        [SerializeField] float heightDrop = 1f, timeDrop = 0.5f, dropRate = 0.12f;
        public override void AnimStartLevel(Action callback)
        {
            List<Transform> arrPiece = new();
            arrItemDissolve.ForEach(x =>
            {
                arrPiece.Add(x.itemDissolve.TF);
            });
            arrPiece.Sort((a, b) => a.position.y.CompareTo(b.position.y));
            StartCoroutine(IEDrop(arrPiece, callback));
        }
        IEnumerator IEDrop(List<Transform> arrPiece, Action callback)
        {
            arrPiece.ForEach(x => x.gameObject.SetActive(false));
            for (int i = 0; i < arrPiece.Count; i++)
            {
                Vector3 defaultPos = arrPiece[i].localPosition;
                arrPiece[i].localPosition = defaultPos + Vector3.up * heightDrop;
                arrPiece[i].gameObject.SetActive(true);
                arrPiece[i].DOLocalMove(defaultPos, timeDrop);
                yield return new WaitForSeconds(dropRate);
            }
            yield return new WaitForSeconds(timeDrop);
            LevelControl.Instance.cameraCtrl.RotateAroundObject(1.5f);
            yield return new WaitForSeconds(1.6f);
            callback?.Invoke();
        }
    }
}