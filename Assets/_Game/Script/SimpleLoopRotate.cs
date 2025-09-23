using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TrungKien
{
    public class SimpleLoopRotate : MonoBehaviour
    {
        [SerializeField] Transform objFan;
        [SerializeField] float rotationSpeed = 45f;
        [SerializeField] float multiSpeed = 1;
        [SerializeField] bool isRotate = true;
        [SerializeField] EAxis axis = EAxis.Y;
        Vector3 direct;
        void Start()
        {
            switch (axis)
            {
                case EAxis.X:
                    direct = Vector3.right;
                    break;
                case EAxis.Y:
                    direct = Vector3.up;
                    break;
                case EAxis.Z:
                    direct = Vector3.forward;
                    break;
            }
        }
        void Update()
        {
            if (isRotate)
            {
                objFan.Rotate(direct * rotationSpeed * multiSpeed * LevelControl.Instance.DetlaTime, Space.Self);
            }
        }
    }
}