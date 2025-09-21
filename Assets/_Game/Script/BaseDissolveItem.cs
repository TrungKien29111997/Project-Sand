using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
namespace TrungKien
{
    public abstract class BaseDissolveItem : PoolingElement
    {
        [SerializeField] MeshRenderer meshRen;
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] Collider col;
        public int id { get; set; }
        Material mat;
        Material Mat { get { return mat ??= meshRen.material; } }
        public void Dissolve()
        {
            Mat.SetColor(Constant.pMainShaderEmissiveColor, DataSystem.Instance.gameplaySO.sandEmissiveColor);
            DOTween.To(x =>
            {
                Mat.SetFloat(Constant.pMainShaderEmissiveStrength, x);
            }, 0, 2, 1).OnComplete(() =>
            {
                EventManager.EmitEvent(Constant.EVENT_UPDATE_UI_GAMEPLAY_DISSOLVE_ITEM_COUNTER);
                SandVFX sandFX = PoolingSystem.Spawn(DataSystem.Instance.prefabSO.dicObjPooling[EPooling.SandFX]) as SandVFX;
                sandFX.SetUp(meshFilter.sharedMesh, TF, LevelControl.Instance.TranDestination);
                Debug.Log("Dissolve");
                gameObject.SetActive(false);
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