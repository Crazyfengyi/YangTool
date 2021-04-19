using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [Autohook]
    public RectTransform GameObject;
    // Start is called before the first frame update
    void Start()
    {
        YangToolDebuger.Debuger.ToLog("a");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
