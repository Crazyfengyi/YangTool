/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2023.2.0b16 
 *创建时间:         2023-12-10 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using Sirenix.Utilities;
using System.Collections.Generic;

public class UIRedDotManager : MonoBehaviour
{
    private static UIRedDotManager instance;
    public static UIRedDotManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("[UIRedDotManager]")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<UIRedDotManager>();
            }

            return instance;
        }
    }
    public static UIRedDotManager DirectInstance => instance;

    private int[] redDotSystemCounters;//红点系统计数器
    private HashSet<long> redDotDetailCounters;//红点细节计数器
    
    private void Awake()
    {
        const int maxCapacity = 1024; //1024个系统差不多就是极限了.自己去定义枚举
        redDotSystemCounters = new int[maxCapacity];
        redDotDetailCounters = new HashSet<long>();
    }
    /// <summary>
    /// 添加未读消息
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="dataIndex">下标</param>
    public void AddUnreadMessage(int[] path, int dataIndex = -1)
    {
        //不允许重复添加，最终一定对应到特定某个原件
        foreach (var i in path)
        {
            ref var a = ref redDotSystemCounters[i];
            a++;
        }

        if (dataIndex >= 0 && !path.IsNullOrEmpty())
        {
            //最后一位
            long longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            redDotDetailCounters.Add(longKey);
        }
    }
    /// <summary>
    /// 移除未读消息
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="dataIndex">下标</param>
    public void RemoveUnreadMessage(int[] path, int dataIndex = -1)
    {
        //不允许重复移除，最终一定对应到特定某个原件
        foreach (var i in path)
        {
            ref var a = ref redDotSystemCounters[i];
            a--;
            if (a < 0) a = 0;
        }

        if (dataIndex >= 0 && !path.IsNullOrEmpty())
        {
            //最后一位
            long longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            redDotDetailCounters.Remove(longKey);
        }
    }
    /// <summary>
    /// 是否有未读消息
    /// </summary>
    public bool HasUnreadMessage(int[] path, int dataIndex = -1)
    {
        if (path.IsNullOrEmpty()) return false;

        if (dataIndex >= 0)
        {
            //最后一位
            long longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            return redDotDetailCounters.Contains(longKey);
        }
        //数组最后一个
        return redDotSystemCounters[path[^1]] > 0;
    }
    /// <summary>
    /// 获得未读消息数量
    /// </summary>
    public int GetUnreadMessageCount(int[] path, int dataIndex = -1)
    {
        if (path.IsNullOrEmpty()) return 0;

        if (dataIndex >= 0)
        {
            //最后一位
            long longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            return redDotDetailCounters.Contains(longKey) ? 1 : 0;
        }

        return redDotSystemCounters[path[^1]];
    }
    
    //事例:
    /*
          private bool RefreshUnreadMessageState()
        {
            if (hasRedDot)
            {
                builtRedDotPath ??= GenerateUnreadMessageSystemPath();
                var has = OrbitUIUnreadMessageManager.Instance.HasUnreadMessage(builtRedDotPath, DataIndex);
                redDotDisplay.enabled = has;
                return has;
            }

            return false;
        }

        private void MarkMessageRead()
        {
            if (hasRedDot && isDisplayingRedDot && OrbitUIUnreadMessageManager.DirectInstance != null)
            {
                builtRedDotPath ??= GenerateUnreadMessageSystemPath();
                OrbitUIUnreadMessageManager.DirectInstance.RemoveUnreadMessage(builtRedDotPath, DataIndex);
            }
        }
     */
}