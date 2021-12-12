using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEditor;
using System.Linq;

public class Test : MonoBehaviour
{
     void Start()
    {
        // TestFun("test");
    }
    public void TestFun(string str, [CallerMemberNameAttribute] string callName = "")
    {
        Debug.LogError($"{callName}");
        List<int> temp = new List<int>();
        temp.ForEach(a =>
        {
        });
    }

}