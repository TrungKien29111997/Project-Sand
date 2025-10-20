using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace TrungKien.Core.VFX
{
    public class SandLine : PoolingElement
    {
        [SerializeField] LineRenderer line;
        [SerializeField] Transform tranDestination;
        Transform tranTarget;
        MaterialPropertyBlock mpb;

        public void SetUp(Vector3 startPoint, Transform tranTarget, float startWidth, Color color)
        {
            line.SetPosition(0, startPoint);
            this.tranTarget = tranTarget;

            // line có 2 điểm: đầu và cuối
            line.startWidth = startWidth;
            line.endWidth = 0.3f;
            line.positionCount = 2;
            mpb = VFXSystem.GetMPB();
            //mpb.SetColor(Constants.pMainColor, color);
            line.SetPropertyBlock(mpb);
        }
        public void Despawn()
        {
            tranDestination.position = TF.position;
            VFXSystem.ReturnMPB(mpb);
        }
        void Update()
        {
            if (tranTarget != null)
            {
                tranDestination.position = Vector3.Lerp(tranDestination.position, tranTarget.position, 0.2f);
                line.SetPosition(1, tranDestination.position); // điểm cuối
            }
        }
#if UNITY_EDITOR
        [Button]
        void Editor()
        {
            line = GetComponent<LineRenderer>();
        }
#endif
    }
}