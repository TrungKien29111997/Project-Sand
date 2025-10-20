using DG.Tweening;
using Sirenix.OdinInspector;
using TrungKien.Core.VFX;
using UnityEngine;

namespace TrungKien.Core.Gameplay
{
    public abstract class BaseDissolveItem : PoolingElement
    {
        [SerializeField] bool isDynamic = false;
        public MeshRenderer meshRen;
        public MeshFilter meshFilter;
        [SerializeField] protected Collider col;
        public Color cacheColor { get; private set; }
        public void CopyInfo(BaseDissolveItem item)
        {
            item.meshRen.sharedMaterial = this.meshRen.sharedMaterial;
        }
        public void SetColor(Color color)
        {
            MaterialPropertyBlock mpb = VFXSystem.GetMPB();
            mpb.SetColor(Constants.pShaderSandColor, color);
            meshRen.SetPropertyBlock(mpb);
            cacheColor = color;
        }

        public void Dissolve(bool isLastLayer)
        {
            col.enabled = false;
            if (!isLastLayer)
            {
                BaseDissolveItem effectObj = PoolingSystem.Spawn(this, TF.position, TF.rotation);
                effectObj.TF.localScale = TF.lossyScale;
                CopyInfo(effectObj);
                Vector3 cacheScale = TF.localScale;
                TF.localScale = new Vector3(cacheScale.x * 0.9f, cacheScale.y * 0.9f, cacheScale.z * 0.9f);
                VFXSystem.SpawnVFX(ETypeVFX.Sand, TF, LevelControl.Instance.GetGizmoPos(gameObject.name), effectObj.meshFilter, effectObj.meshRen, cacheColor, () =>
                {
                    PoolingSystem.Despawn(effectObj);
                    TF.DOScale(cacheScale, 0.5f).OnComplete(() => col.enabled = true);
                }, () =>
                {
                    LevelControl.Instance.ItemDissolve(gameObject.name);
                });
            }
            else
            {
                VFXSystem.SpawnVFX(ETypeVFX.Sand, TF, LevelControl.Instance.GetGizmoPos(gameObject.name), meshFilter, meshRen, cacheColor, () =>
                {
                    gameObject.SetActive(false);
                }, () =>
                {
                    LevelControl.Instance.ItemDissolve(gameObject.name);
                });
            }
        }

        // public void Warning()
        // {
        //     col.enabled = false;
        //     Blink(Color.red, 1, 2, 0.25f, () => col.enabled = true);
        // }
        // void Blink(Color color, float strength, int loop = 2, float time = 0.2f, System.Action doneAction = null)
        // {
        //     Mat.SetColor(Constant.pMainShaderEmissiveColor, color);
        //     Sequence seq = DOTween.Sequence();
        //     for (int j = 0; j < loop; j++)
        //     {
        //         seq.Append(DOTween.To(x => Mat.SetFloat(Constant.pMainShaderEmissiveStrength, x), 0, strength, time));
        //         seq.Append(DOTween.To(x => Mat.SetFloat(Constant.pMainShaderEmissiveStrength, x), strength, 0, time));
        //     }
        //     Fix.DelayedCall(loop * time * 2 + 1, () => doneAction?.Invoke());
        // }
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