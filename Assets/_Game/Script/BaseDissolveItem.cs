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
        [SerializeField] protected MeshRenderer meshRen;
        [SerializeField] protected MeshFilter meshFilter;
        [SerializeField] protected Collider col;
        public int id { get; set; }
        Material mat;
        Material Mat { get { return mat ??= meshRen.material; } }
        bool isDissolving = false;
        float heightCut;
        float boundSize;
        public void Dissolve()
        {
            isDissolving = true;
            boundSize = Vector3.Scale(meshFilter.sharedMesh.bounds.size, meshFilter.transform.lossyScale).y;
            col.enabled = false;
            float defaultLerp = Mat.GetFloat("_Lerp");
            DOTween.To(x =>
            {
                Mat.SetFloat("_Lerp", x);
            }, defaultLerp, 0, 1).OnComplete(() =>
            {
                DOTween.To(x =>
                {
                    heightCut = x;
                }, 0, boundSize, 3).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
                SoundManager.Instance.PlaySound(DataSystem.Instance.gameplaySO.sfxBling);
                SandVFX sandFX = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.SandFX]) as SandVFX;
                sandFX.SetUp(meshFilter.sharedMesh, Extension.GetMinHeightApprox(meshFilter), Extension.GetMaxHeightApprox(meshFilter), TF, LevelControl.Instance.TranDestination);
                Debug.Log("Dissolve");
            });
        }
        void Update()
        {
            if (isDissolving)
            {
                Mat.SetFloat(Constant.pMainShaderCutOffHeight, TF.position.y + (boundSize / 2) - heightCut);
            }
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