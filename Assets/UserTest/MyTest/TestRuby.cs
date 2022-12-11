using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using YangTools.UGUI;
using YangTools;

public class TestRuby : MonoBehaviour
{
    void Start()
    {

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var textScript = GetComponentInChildren<CustomText>();
            textScript.ShowTextByTyping(textScript.text, () =>
            {
                Debug.LogError("≤‚ ‘Ω· ¯..");
            });
        }
    }

}
