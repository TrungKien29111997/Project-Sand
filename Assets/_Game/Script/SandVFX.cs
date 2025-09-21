using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace TrungKien
{
    public class SandVFX : PoolingElement
    {
        [SerializeField] VisualEffect vfx;
        [SerializeField] MyVFXTransformBinder[] arrTransformBinder;
        Dictionary<string, MyVFXTransformBinder> dicTransformBinder;
        void Awake()
        {
            dicTransformBinder = new();
            for (int i = 0; i < arrTransformBinder.Length; i++)
            {
                dicTransformBinder.Add(arrTransformBinder[i].Property, arrTransformBinder[i]);
            }
        }
        public void SetUp(Mesh mesh, Transform objectTransform, Transform target)
        {
            vfx.SetMesh(Constant.pMesh, mesh);
            dicTransformBinder[Constant.pTranActiveVFXSand].Target = objectTransform;
            dicTransformBinder[Constant.pTranTargetVFXSand].Target = target;
            vfx.Play();
            StartCoroutine(IEDestroy());
        }
        IEnumerator IEDestroy()
        {
            yield return new WaitUntil(() => vfx.aliveParticleCount == 0);
            PoolingSystem.Despawn(this);
        }
#if UNITY_EDITOR
        [Button]
        void Editor()
        {
            arrTransformBinder = GetComponents<MyVFXTransformBinder>();
        }
#endif
    }
}