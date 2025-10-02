using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;
namespace TrungKien
{
    public class FXFlyOut : PoolingElement
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
        public void SetUp(Color sandColor, int spawnFator, float delay, Transform objectTransform, Transform target)
        {
            DOTween.To(x => vfx.SetFloat("Alpha", x), 0, 1, delay).SetEase(Ease.Linear);
            vfx.SetVector4("StartColor", (Vector4)sandColor.linear);
            vfx.SetInt("SpawnCount", spawnFator);
            vfx.SetFloat("DelayTime", delay);
            vfx.SetVector2("LifeTime", new Vector2(delay + 1.5f, delay + 3f));
            dicTransformBinder[Constant.pTranActiveVFXSand].Target = objectTransform;
            dicTransformBinder[Constant.pTranTargetVFXSand].Target = target;
            vfx.Play();
            StartCoroutine(IEDestroy());
        }
        IEnumerator IEDestroy()
        {
            yield return new WaitUntil(() => vfx.aliveParticleCount == 0);
            ++LevelControl.Instance.ItemCounter;
            EventManager.EmitEvent(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER);
            PoolingSystem.Despawn(this);
            vfx.SetFloat("Alpha", 0);
            vfx.Stop();
        }
    }
}