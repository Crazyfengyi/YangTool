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
    private int[] redDotSystemCounters;
    private HashSet<long> redDotDetailCounters;

    private static UIRedDotManager m_Instance;
    public static UIRedDotManager DirectInstance => m_Instance;

    public static UIRedDotManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                var obj = new GameObject("[UIRedDotManager]")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                DontDestroyOnLoad(obj);
                m_Instance = obj.AddComponent<UIRedDotManager>();
            }

            return m_Instance;
        }
    }

    private void Awake()
    {
        const int maxCapacity = 1024; //1024个系统差不多就是极限了.自己去定义枚举
        redDotSystemCounters = new int[maxCapacity];
        redDotDetailCounters = new HashSet<long>();
    }

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
            var longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            redDotDetailCounters.Add(longKey);
        }
    }

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
            var longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            redDotDetailCounters.Remove(longKey);
        }
    }

    public bool HasUnreadMessage(int[] path, int dataIndex = -1)
    {
        if (path.IsNullOrEmpty()) return false;

        if (dataIndex >= 0)
        {
            var longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            return redDotDetailCounters.Contains(longKey);
        }

        return redDotSystemCounters[path[^1]] > 0;
    }

    public int GetUnreadMessageCount(int[] path, int dataIndex = -1)
    {
        if (path.IsNullOrEmpty()) return 0;

        if (dataIndex >= 0)
        {
            var longKey = ((long)path[^1] << 32) | (long)(dataIndex);
            return redDotDetailCounters.Contains(longKey) ? 1 : 0;
        }

        return redDotSystemCounters[path[^1]];
    }
}