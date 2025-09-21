using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrungKien
{
    public class PoolingElement : MonoBehaviour
    {
        private Transform tf;
        public Transform TF
        {
            get
            {
                return tf ??= transform;
            }
        }
        int instanceID;
        public int InstanceID { get => instanceID; set => instanceID = value; }
        public virtual void PoolSetup() { }
    }
}