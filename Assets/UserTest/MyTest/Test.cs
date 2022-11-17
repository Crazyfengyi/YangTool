using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using YangTools.UGUI;

public class Test : MonoBehaviour
{
    public Material material;
    public float value;
    public string testStr;
    public TestSo testSo;
    void Start()
    {
        //Create();
        //Create2();
        //YangTools.Log.Debuger.IsOpenLog = true;

        //for (int i = 0; i < 10000; i++)
        //{
        //    YangTools.Log.Debuger.ToError($"{i}");
        //}

        //var settings = new ES3Settings(ES3.EncryptionType.AES, "myPassword");
        //ES3.Save<int>("key1", 123, settings);
        //int test = ES3.Load<int>("key1", settings);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //var textScript = GetComponentInChildren<CustomText>();
            //textScript.ShowTextByTyping(textScript.text);
            UIMonoInstance.Instance.OpenUIPanel("DialoguePanel", "One");
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //material.SetFloat("_Value", value);
        //Graphics.Blit(source, destination, material);
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

    #region 
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