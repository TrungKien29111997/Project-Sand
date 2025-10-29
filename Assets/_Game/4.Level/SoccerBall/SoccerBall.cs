using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace TrungKien.Core.Gameplay
{
    public class SoccerBall : BaseTargetObject
    {
        [SerializeField] float timeMove = 3f, ballRaius;
        [SerializeField] int amountRoll = 3;
        [SerializeField] Transform tranCenter;
        bool isRotate;
        float lenght => ballRaius * amountRoll * 2 * Mathf.PI;
        public override void AnimStartLevel(Action callback)
        {
            StartCoroutine(IEAnim(callback));
        }
        IEnumerator IEAnim(Action callback)
        {
            arrItemDissolve.ForEach(x =>
            {
                MaterialPropertyBlock mpb = x.itemDissolve.currentMpb;
                mpb.SetFloat("_Roughness", 1f);
                mpb.SetFloat("_NormalStrength", 0f);
                x.itemDissolve.meshRen.SetPropertyBlock(mpb);
            });
            isRotate = true;
            TF.position = Vector3.forward * lenght;
            TF.DOMove(Vector3.zero, timeMove).SetEase(Ease.Linear).OnComplete(() => isRotate = false);
            yield return new WaitForSeconds(timeMove + 0.5f);
            arrItemDissolve.ForEach(x =>
            {
                MaterialPropertyBlock mpb = x.itemDissolve.currentMpb;
                DOTween.To(y =>
                {
                    mpb.SetFloat("_Roughness", y);
                    x.itemDissolve.meshRen.SetPropertyBlock(mpb);
                }, 1f, 0.2f, 1f);
                DOTween.To(z =>
                {
                    mpb.SetFloat("_NormalStrength", z);
                    x.itemDissolve.meshRen.SetPropertyBlock(mpb);
                }, 0f, 5f, 1f);
            });
            LevelControl.Instance.cameraCtrl.RotateAroundObject(1.5f);
            tranCenter.DOLocalRotate(Vector3.zero, 1.5f);
            yield return new WaitForSeconds(1.6f);
            callback?.Invoke();
        }
        void Update()
        {
            if (!isRotate) return;
            // Quãng đường di chuyển trong frame này
            float distance = (lenght / timeMove) * Time.deltaTime;

            // Di chuyển bóng
            TF.position += -Vector3.forward * distance;

            // Tính góc quay (độ)
            float angle = distance / ballRaius * Mathf.Rad2Deg;

            // Xác định trục xoay (vuông góc với hướng di chuyển và mặt đất)
            Vector3 rotationAxis = Vector3.Cross(Vector3.forward, Vector3.up);

            // Quay bóng quanh trục này
            tranCenter.Rotate(rotationAxis, angle, Space.World);
        }
    }
}