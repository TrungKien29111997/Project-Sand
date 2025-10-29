using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using DG.Tweening;
using Sirenix.OdinInspector;
using TrungKien.Core.VFX;
using UnityEngine;
namespace TrungKien.Core.Gameplay
{
    public class Dinosaur : BaseTargetObject
    {
        [SerializeField] Color colorEye, colorEyeLid;
        [SerializeField] MeshRenderer[] arrMeshEye, arrMeshEyeLid;
        [SerializeField] Transform tranRightEyeLid, tranLeftEyeLid;
        [SerializeField] float lengthMove, jumpPower, jumpDuration, squashAmount, squashTime;
        [SerializeField] int amountJump;
        List<MaterialPropertyBlock> listMpbEye, listMpbEyeLid;
        public override void AnimStartLevel(System.Action callback)
        {
            listMpbEye = new();
            listMpbEyeLid = new();
            arrItemDissolve.ForEach(x =>
            {
                MaterialPropertyBlock mpb = x.itemDissolve.currentMpb;
                mpb.SetFloat("_Roughness", 1f);
                mpb.SetFloat("_NormalStrength", 0f);
                x.itemDissolve.meshRen.SetPropertyBlock(mpb);
            });
            arrMeshEye.ForEach(x =>
            {
                MaterialPropertyBlock mpb = VFXSystem.GetMPB();
                mpb.SetColor(Constants.pLitBaseColor, colorEye);
                mpb.SetFloat(Constants.pDither, 0f);
                x.SetPropertyBlock(mpb);
                listMpbEye.Add(mpb);
            });
            arrMeshEyeLid.ForEach(x =>
            {
                MaterialPropertyBlock mpb = VFXSystem.GetMPB();
                mpb.SetColor(Constants.pLitBaseColor, colorEyeLid);
                mpb.SetFloat(Constants.pDither, 0f);
                x.SetPropertyBlock(mpb);
                listMpbEyeLid.Add(mpb);
            });
            TF.position = Vector3.forward * lengthMove;
            Jump(callback);
        }
        void Jump(System.Action callback)
        {
            TF.DOKill(true);
            TF.localScale = Vector3.one;

            // Bắt đầu tween nhảy
            TF.DOJump(Vector3.zero, jumpPower, amountJump, jumpDuration)
             .SetEase(Ease.Linear)
             .OnStart(() =>
             {
                 Debug.Log("Bắt đầu nhảy!");
                 // Chuẩn bị hiệu ứng nhún theo số lần nhảy
                 float timePerJump = jumpDuration / amountJump;
                 for (int i = 1; i <= amountJump; i++)
                 {
                     float delay = timePerJump * i;
                     DOVirtual.DelayedCall(delay - squashTime, () =>
                     {
                         // Scale nhún xuống rồi trở lại
                         TF.DOScaleY(squashAmount, squashTime)
                          .SetEase(Ease.InOutQuad)
                          .OnComplete(() =>
                          {
                              TF.DOScaleY(1f, squashTime).SetEase(Ease.OutQuad);
                          });
                     });
                 }
             })
             .OnComplete(() =>
             {
                 DebugCustom.Log("Đã đến đích!");
                 TF.localScale = Vector3.one; // reset scale
                 BlinkEye(callback);
             });
        }
        void BlinkEye(System.Action callback)
        {
            tranLeftEyeLid.DOLocalRotate(new Vector3(0f, 180, -35f), 0.2f).OnComplete(() =>
            {
                tranLeftEyeLid.DOLocalRotate(new Vector3(0f, 180, -160f), 0.3f);
            });
            tranRightEyeLid.DOLocalRotate(new Vector3(0f, 180, -130f), 0.2f).OnComplete(() =>
            {
                tranRightEyeLid.DOLocalRotate(new Vector3(0f, 180, -20f), 0.3f);
            });
            Fix.DelayedCall(0.5f, () => LevelControl.Instance.cameraCtrl.RotateAroundObject(1.5f));
            Fix.DelayedCall(1f, () => ChangeMaterial(callback));
        }
        void ChangeMaterial(System.Action callback)
        {
            for (int i = 0; i < arrMeshEye.Length; i++)
            {
                MaterialPropertyBlock mpb = listMpbEye[i];
                MeshRenderer meshEye = arrMeshEye[i];
                DOTween.To(y =>
                {
                    mpb.SetFloat(Constants.pDither, y);
                    meshEye.SetPropertyBlock(mpb);
                }, 0f, 2f, 1f);
            }
            for (int i = 0; i < arrMeshEyeLid.Length; i++)
            {
                MaterialPropertyBlock mpb = listMpbEyeLid[i];
                MeshRenderer meshEyeLid = arrMeshEyeLid[i];
                DOTween.To(y =>
                {
                    mpb.SetFloat(Constants.pDither, y);
                    meshEyeLid.SetPropertyBlock(mpb);
                }, 0f, 2f, 1f);
            }

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
            Fix.DelayedCall(1.2f, () =>
            {
                listMpbEye.ForEach(x => VFXSystem.ReturnMPB(x));
                listMpbEyeLid.ForEach(x => VFXSystem.ReturnMPB(x));
                arrMeshEye.ForEach(x => x.gameObject.SetActive(false));
                arrMeshEyeLid.ForEach(x => x.gameObject.SetActive(false));
                callback?.Invoke();
            });
        }
    }
}