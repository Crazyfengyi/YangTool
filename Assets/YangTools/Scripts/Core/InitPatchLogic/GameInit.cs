/*
 *Copyright(C) 2020 by Test 
 *All rights reserved.
 *Author:WIN-VJ19D9AB7HB 
 *UnityVersion：6000.0.23f1c1 
 *创建时间:2025-06-28 
 */  
using System;
using System.Collections;
using GameMain;
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using YangTools;
using YooAsset;

public class GameInit : MonoBehaviour
{
    public static GameInit Instance;
    private EPlayMode playMode;
    
    public void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
    
    private IEnumerator Start()
    {
        // 初始化资源系统
        YooAssets.Initialize();
        // 开始补丁更新流程
        var operation = new PatchOperation("DefaultPackage","",playMode);
        YooAssets.StartOperation(operation);
        yield return operation;

        // 设置默认的资源包
        var gamePackage = YooAssets.GetPackage("DefaultPackage");
        YooAssets.SetDefaultPackage(gamePackage);
        yield return null;

        SceneManager.LoadSceneAsync("GameEntry");
    }

    public void InitializeAfterManagers()
    {
        
    }
} 