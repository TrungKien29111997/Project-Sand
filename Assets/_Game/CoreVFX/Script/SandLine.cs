using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
namespace TrungKien.Core.VFX
{
    public class SandLine : PoolingElement
    {
        [SerializeField] SkinnedMeshRenderer meshRen;
        public List<Transform> listTranPoint;
        [SerializeField] Transform root;
        Transform tranTarget;
        MaterialPropertyBlock mpb;
        Vector3 upVector = Vector3.up;

        public void SetUp(Vector3 startPos, Vector3 upVector, Transform tranTarget, float startWidth, Color color, float timeDestroy)
        {
            this.upVector = upVector;
            this.tranTarget = tranTarget;
            TF.position = startPos;
            mpb = VFXSystem.GetMPB();
            mpb.SetColor(Constants.pMainColor, color);
            DOTween.To(x =>
            {
                mpb.SetFloat("_Maskmultiplayer", x);
                meshRen.SetPropertyBlock(mpb);
            }, 0f, 6f, (timeDestroy / 2));
            Fix.DelayedCall(timeDestroy / 2, () =>
            {
                float maskValue = 6f;
                DOTween.To(() => maskValue, x =>
                {
                    maskValue = x;
                    mpb.SetFloat("_Maskmultiplayer", maskValue);
                    meshRen.SetPropertyBlock(mpb);
                }, 0f, timeDestroy / 2);
            });
            
            TF.LookAtAroundUp(tranTarget.position);
            root.localScale = Vector3.one * startWidth;
            //ApplyHierarchyLossyScale(listTranPoint, Vector3.one * startWidth, Vector3.one * 0.3f);
            Fix.DelayedCall(timeDestroy, Despawn);
        }
        void Despawn()
        {
            for (int i = 1; i < listTranPoint.Count; i++)
            {
                listTranPoint[i].position = TF.position;
            }
            VFXSystem.ReturnMPB(mpb);
            PoolingSystem.Despawn(this);
        }
        List<Vector3> listCurvePos;
        void Update()
        {
            if (tranTarget != null)
            {
                listTranPoint[^1].position = Vector3.Lerp(listTranPoint[^1].position, tranTarget.position, 0.2f);
                listCurvePos = Extension.GetPointCurve(upVector, TF.position, tranTarget.position, 0.5f, listTranPoint.Count);
                for (int i = 0; i < listCurvePos.Count; i++)
                {
                    listTranPoint[i].position = listCurvePos[i];
                }
            }
        }
#if UNITY_EDITOR
        [Button]
        void Editor()
        {
            meshRen = GetComponentInChildren<SkinnedMeshRenderer>();
        }
#endif
        void ApplyHierarchyLossyScale(List<Transform> listPoint, Vector3 startLossy, Vector3 endLossy)
        {
            if (listPoint == null || listPoint.Count < 2)
            {
                Debug.LogWarning("Cần ít nhất 2 điểm.");
                return;
            }

            // Gán localScale điểm đầu (giả định cha của nó scale (1,1,1))
            listPoint[0].localScale = startLossy;

            int n = listPoint.Count;
            // Mỗi điểm sẽ có target lossyScale tăng đều
            for (int i = 1; i < n; i++)
            {
                float t = i / (float)(n - 1);
                Vector3 targetLossy = Vector3.Lerp(startLossy, endLossy, t);

                // Lấy lossyScale của cha
                Vector3 parentLossy = listPoint[i - 1].lossyScale;

                // Tính localScale cần thiết để đạt target lossyScale
                Vector3 local = new Vector3(
                    SafeDiv(targetLossy.x, parentLossy.x),
                    SafeDiv(targetLossy.y, parentLossy.y),
                    SafeDiv(targetLossy.z, parentLossy.z)
                );

                listPoint[i].localScale = local;
            }

            // Bây giờ lossyScale cuối cùng = endLossy (hoặc sai rất nhỏ do float)
            DebugCustom.Log($"LossyScale cuối cùng = {listPoint[^1].lossyScale}");

            float SafeDiv(float a, float b)
            {
                return (Mathf.Abs(b) < 1e-6f) ? 0f : a / b;
            }
        }
    }
}