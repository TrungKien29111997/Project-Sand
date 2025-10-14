using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace TrungKien.UI
{
    public class CanvasLoading : UICanvas
    {
        [SerializeField] Transform[] arrDoor;
        [SerializeField] AudioClip sfxClose;
        public void SetCanvas(System.Action callback)
        {
            StartCoroutine(IEOpenDoor(callback));
        }
        IEnumerator IEOpenDoor(System.Action callback)
        {
            List<float> listDefaultPos = new();
            arrDoor.ForEach(x => listDefaultPos.Add(x.localPosition.x));
            for (int i = 0; i < arrDoor.Length; i++)
            {
                Transform x = arrDoor[i];
                x.DOLocalMoveX(0f, 1f);
            }
            yield return new WaitForSeconds(1.1f);
            SoundManager.Instance.PlaySound(sfxClose);
            callback?.Invoke();
            yield return new WaitForSeconds(0.9f);
            for (int i = 0; i < arrDoor.Length; i++)
            {
                float defaulPos = listDefaultPos[i];
                Transform x = arrDoor[i];
                x.DOLocalMoveX(defaulPos, 1.5f);
            }
            yield return new WaitForSeconds(1.6f);
            Close();
        }
    }
}