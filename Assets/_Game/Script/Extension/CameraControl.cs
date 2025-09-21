using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TrungKien
{
    public class CameraControl : MonoBehaviour
    {
        Transform tf;
        public Transform TF { get { return tf ??= transform; } }
        public Camera camera;
        [SerializeField] float min = 5f, max = 6.25f;
        [SerializeField] AnimationCurve cameraShakeCurve;
        public float valueSizeCamera { get; private set; }
        // Start is called before the first frame update
        void Start()
        {
            float target = Extension.GetValue((float)Screen.height / (float)Screen.width, 1920f / 1080f, 1600f / 720f, min, max);
            StartCoroutine(IEFixCamera(5, target));
        }

        private IEnumerator IEFixCamera(float start, float target)
        {
            yield return null;
            float time = 0;
            while (time < 0.2f)
            {
                time += Time.deltaTime;
                if (time >= 0.2f) time = 0.2f;
                valueSizeCamera = Mathf.Lerp(start, target, time / 0.2f);
                BackToDefaultSize();
            }
        }

        public void OnShake()
        {
            Vector3 defaultPos = transform.position;
            DOTween.To(x =>
            {
                transform.position = defaultPos + Vector3.up * cameraShakeCurve.Evaluate(x);
            }, 0, 1, 1.5f);
        }
        public void BackToDefaultSize()
        {
            ChangeOrthographicSize(valueSizeCamera);
        }
        public void ChangeOrthographicSize(float size, float time = 1f)
        {
            camera.DOOrthoSize(size, time);
        }
    }
}