/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-12 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Scripts.Core.YangCoroutine
{
    /// <summary>
    /// 协程管理器(利用迭代器实现携程)
    /// </summary>
    public class YangCoroutineManager
    {
        private static YangCoroutineManager instance;
        public static YangCoroutineManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new YangCoroutineManager();
                }
                return instance;
            }
        }
        //链表
        private readonly LinkedList<IEnumerator> coroutineList = new LinkedList<IEnumerator>();
        /// <summary>
        /// 开始协程
        /// </summary>
        public void StartCoroutine(IEnumerator ie)
        {
            coroutineList.AddLast(ie);
        }
        /// <summary>
        /// 停止协程
        /// </summary>
        public void StopCoroutine(IEnumerator ie)
        {
            try
            {
                coroutineList.Remove(ie);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        public void UpdateCoroutine()
        {
            LinkedListNode<IEnumerator> node = coroutineList.First;
            while (node != null)
            {
                IEnumerator ie = node.Value;
                bool result = true;
                if (ie.Current is IWait)
                {
                    IWait wait = ie.Current as IWait;
                    if (wait != null && wait.Tick())
                    {
                        result = ie.MoveNext();
                    }
                }
                else
                {
                    result = ie.MoveNext();
                }

                if (!result)
                {
                    coroutineList.Remove(node);
                }

                node = node.Next;
            }
        }
    }
    /// <summary>
    /// 等待接口
    /// </summary>
	public interface IWait
    {
        bool Tick();
    }
    /// <summary>
    /// 等待秒
    /// </summary>
    public class YangWaitSecond : IWait
    {
        float seconds = 0f;
        public YangWaitSecond(float _seconds)
        {
            seconds = _seconds;
        }
        public bool Tick()
        {
            seconds -= Time.deltaTime;
            return seconds <= 0;
        }
    }
}

/*
	例:
    YangCoroutineManager.Instance.StartCoroutine(TestMyIe());

    public void Update()
    {
        YangCoroutineManager.Instance.Updatecorountine();
    }
    public IEnumerator TestMyIe()
    {
        Debug.LogError("TestMyIe1");
        yield return new YangWaitSecond(2);
        Debug.LogError("TestMyIe2");
    }
*/