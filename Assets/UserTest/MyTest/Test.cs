using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEditor;
using System.Linq;
using YangTools;
using TreeEditor;

public class Test : MonoBehaviour
{
    void Start()
    {
        //Create();
        //Create2();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var textScript = GetComponentInChildren<CustomText>();
            textScript.ShowTextByTyping(textScript.text);
        }
    }
    IEnumerator Tets2()
    {
        yield return new WaitWhile(() => { return true; });
    }
    public void TestFun(string str, [CallerMemberNameAttribute] string callName = "")
    {
        Debug.LogError($"{callName}");
        List<int> temp = new List<int>();
        temp.ForEach(a =>
        {
        });
    }

    #region 实例化测试
    /// <summary>
    /// 直接调用
    /// </summary>
    public static void Create()
    {
        Person person = null;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        for (int i = 0; i < 100000; i++)
            person = new Person();
        watch.Stop();
        Debug.LogError("new:" + watch.ElapsedMilliseconds.ToString().PadLeft(5));
    }
    /// <summary>
    /// 实例化反射
    /// </summary>
    public static void Create2()
    {
        Type type = typeof(Person);
        Person person = null;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        for (int i = 0; i < 100000; i++)
        {
            object obj = Activator.CreateInstance(type);
            person = obj as Person;
        }
        watch.Stop();
        Debug.LogError("Activator:" + watch.ElapsedMilliseconds.ToString().PadLeft(5));
    }
    #endregion
}

public class Person
{
    public Person()
    {

    }
}