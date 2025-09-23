using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
namespace TrungKien
{
    public abstract class BaseDissolveItem : PoolingElement
    {
        [SerializeField] bool isDynamic = false;
        [SerializeField] protected MeshRenderer meshRen;
        [SerializeField] protected MeshFilter meshFilter;
        [SerializeField] protected Collider col;
        public int id { get; set; }
        Material mat;
        Material Mat { get { return mat ??= meshRen.material; } }
        Vector2 minMaxHeight;
        public void Dissolve()
        {
            minMaxHeight = Extension.GetMinMaxHeightApprox(meshFilter);
            col.enabled = false;
            float defaultLerp = Mat.GetFloat("_Lerp");
            DOTween.To(x =>
            {
                Mat.SetFloat("_Lerp", x);
            }, defaultLerp, 0, 1).OnComplete(() =>
            {
                if (isDynamic)
                {
                    LevelControl.Instance.scaleTime = 0;
                }
                DOTween.To(x =>
                {
                    Mat.SetFloat(Constant.pMainShaderCutOffHeight, x);
                }, minMaxHeight.y, minMaxHeight.x, (minMaxHeight.y - minMaxHeight.x) * DataSystem.Instance.gameplaySO.delayFactor).SetEase(Ease.Linear).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    if (isDynamic)
                    {
                        LevelControl.Instance.scaleTime = 1;
                    }
                });
                SoundManager.Instance.PlaySound(DataSystem.Instance.gameplaySO.sfxBling);
                SandVFX sandFX = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.SandFX]) as SandVFX;
                sandFX.SetUp(meshFilter.sharedMesh, minMaxHeight.x, minMaxHeight.y, TF, LevelControl.Instance.TranDestination);
                Debug.Log("Dissolve");
            });
        }
        public void Warning()
        {
            col.enabled = false;
            Blink(Color.red, 1, 2, 0.25f, () => col.enabled = true);
        }
        void Blink(Color color, float strength, int loop = 2, float time = 0.2f, System.Action doneAction = null)
        {
            Mat.SetColor(Constant.pMainShaderEmissiveColor, color);
            Sequence seq = DOTween.Sequence();
            for (int j = 0; j < loop; j++)
            {
                seq.Append(DOTween.To(x => Mat.SetFloat(Constant.pMainShaderEmissiveStrength, x), 0, strength, time));
                seq.Append(DOTween.To(x => Mat.SetFloat(Constant.pMainShaderEmissiveStrength, x), strength, 0, time));
            }
            Fix.DelayedCall(loop * time * 2 + 1, () => doneAction?.Invoke());
        }
        // void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.green;
        //     if (meshFilter != null && meshFilter.sharedMesh != null)
        //     {
        //         var mesh = meshFilter.sharedMesh;
        //         var bounds = mesh.bounds;

        //         // local to world
        //         Matrix4x4 m = meshFilter.transform.localToWorldMatrix;
        //         Gizmos.matrix = m;
        //         Gizmos.DrawWireCube(bounds.center, bounds.size);
        //         Gizmos.matrix = Matrix4x4.identity; // reset
        //     }
        // }
#if UNITY_EDITOR
        [Button]
        public void Editor()
        {
            meshRen = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            col = GetComponent<Collider>();
        }

#endif
    }
}