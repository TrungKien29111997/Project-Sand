using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    [SerializeField] Transform tranObj, tranCam;

    void LateUpdate()
    {
        tranObj.LookAt(tranCam);
    }
}
