using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWindow : MonoBehaviour
{
    [SerializeField] ConfigGraphSO configSO;
    // Start is called before the first frame update
    void Start()
    {
        configSO.OnNodeSelected += DebugTest;
    }
    void DebugTest(int index)
    {
        Debug.Log(index.ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
