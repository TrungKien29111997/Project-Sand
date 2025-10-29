using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace TrungKien.Core.Gameplay
{
    public class Avocado : BaseTargetObject
    {
        [SerializeField] float heightDrop = 5f, timeDrop = 1f;
        public override void AnimStartLevel(Action callback)
        {
            StartCoroutine(IEDrop(callback));
        }
        IEnumerator IEDrop(Action callback)
        {
            TF.position += Vector3.up * heightDrop;
            yield return new WaitForSeconds(0.2f);
            TF.DOJump(Vector3.zero, 2f, 1, timeDrop).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(timeDrop - 0.2f);
            TF.DOScaleY(0.7f, 0.2f);
            yield return new WaitForSeconds(0.2f);
            TF.DOScaleY(1.2f, 0.4f);
            yield return new WaitForSeconds(0.4f);
            TF.DOScaleY(1f, 0.3f);
            LevelControl.Instance.cameraCtrl.RotateAroundObject(1.5f);
            yield return new WaitForSeconds(1.6f);
            callback?.Invoke();
        }
    }
}