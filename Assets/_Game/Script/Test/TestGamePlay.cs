using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
public class TestGamePlay : MonoBehaviour
{
    [SerializeField] Transform tran1, tran2;
    [Button]
    void Test()
    {
        Debug.Log($"pos1 {tran1.position.x} {tran1.position.y}");
        Debug.Log($"pos2 {tran2.position.x} {tran2.position.y}");
    }
}
#endif