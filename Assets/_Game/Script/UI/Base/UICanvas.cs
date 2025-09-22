using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace TrungKien.UI
{
    public class UICanvas : MonoBehaviour
    {
        private RectTransform rectTf;
        public RectTransform RecTF
        {
            get
            {
                return rectTf ??= GetComponent<RectTransform>();
            }
        }
        [SerializeField] bool isDestroyOnClose = false;
        public System.Type typeParent { get; protected set; }

        private void Awake()
        {
            // xu ly title
            float ratio = (float)Screen.width / (float)Screen.height;
            if (ratio > 2.1f)
            {
                Vector2 leftBottom = RecTF.offsetMin;
                Vector2 rightTop = RecTF.offsetMax;

                leftBottom.y = 0f;
                rightTop.y = -100f;

                RecTF.offsetMin = leftBottom;
                RecTF.offsetMax = rightTop;
            }
        }

        // goi truoc khi canvas duoc active
        public virtual void SetUp()
        {
            SetLeft(0);
            SetRight(0);
            SetTop(0);
            SetBottom(0);
        }
        protected T RequireCanvas<T>() where T : UICanvas
        {
            T canvas = UIManager.Instance.OpenUI<T>();
            typeParent = canvas.GetType();
            return canvas;
        }

        // goi sau khi canvas duoc active
        public virtual void Open()
        {
            gameObject.SetActive(true);
            RecTF.SetAsLastSibling();
            OpenEffect();
        }
        public virtual void OpenEffect()
        {
            //SoundManager.Instance.PlaySound(ESound.SelectUI1);
        }
        public virtual void Close()
        {
            CloseDirecly();
        }

        // tat canvas truc tiep 
        void CloseDirecly()
        {
            if (isDestroyOnClose)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
            UIManager.Instance.CloseChildren(this.GetType());
        }

        public virtual void OnInit()
        {

        }

        protected void SetLeft(float left)
        {
            RecTF.offsetMin = new Vector2(left, RecTF.offsetMin.y);
        }

        protected void SetRight(float right)
        {
            RecTF.offsetMax = new Vector2(-right, RecTF.offsetMax.y);
        }

        protected void SetTop(float top)
        {
            RecTF.offsetMax = new Vector2(RecTF.offsetMax.x, -top);
        }

        protected void SetBottom(float bottom)
        {
            RecTF.offsetMin = new Vector2(RecTF.offsetMin.x, bottom);
        }
        public virtual string GetName()
        {
            return this.name;
        }
    }
}
