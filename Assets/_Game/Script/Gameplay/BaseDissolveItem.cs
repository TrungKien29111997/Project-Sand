using DG.Tweening;
using Sirenix.OdinInspector;
using TrungKien.Core.UI;
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
        Vector3 partLossyScale;
        void Start()
        {
            partLossyScale = TF.lossyScale;
        }
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
            System.Tuple<Transform, System.Action> tupleBowl = LevelControl.Instance.PreFillBowl(gameObject.name);
            Transform targetTran = tupleBowl.Item1;
            if (!isLastLayer)
            {
                Vector3 cacheScale = TF.localScale;
                TF.localScale = new Vector3(cacheScale.x * 0.9f, cacheScale.y * 0.9f, cacheScale.z * 0.9f);
                BaseDissolveItem effectObj = PoolingSystem.Spawn(this, TF.position, TF.rotation);
                effectObj.TF.localScale = partLossyScale;
                CopyInfo(effectObj);
                VFXSystem.SpawnVFX(ETypeVFX.Sand, TF, targetTran, effectObj.meshFilter, effectObj.meshRen, cacheColor, () =>
                {
                    PoolingSystem.Despawn(effectObj);
                    TF.DOScale(cacheScale, 0.5f).OnComplete(() => col.enabled = true);
                    tupleBowl.Item2?.Invoke();
                }, () =>
                {
                    EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_UPDATE_SCORE);
                });
                SetColor(LevelControl.Instance.GetNewColorAndClearOldColor(gameObject.name));
            }
            else
            {
                VFXSystem.SpawnVFX(ETypeVFX.Sand, TF, targetTran, meshFilter, meshRen, cacheColor, () =>
                {
                    gameObject.SetActive(false);
                    tupleBowl.Item2?.Invoke();
                }, () =>
                {
                    EventManager.EmitEvent(Constant.EVENT_GAMEPLAY_UPDATE_SCORE);
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