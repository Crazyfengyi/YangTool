/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using UnityEngine;
using System.Collections;
using System;

namespace YangTools
{
    /// <summary>
    /// 简单的线程安全单例模板
    /// </summary>
    public abstract class SimpleSingleton<T> where T : class, new()
    {
        private static T instance = default(T);
        //线程锁
        private static readonly object obj = new object();
        public T Instance
        {
            get
            {
                lock (obj)
                {
                    if (instance == null)
                    {
                        instance = new T();
                        ////支持非公共的无参构造函数，new T()不支持非公共的无参构造函数
                        //instance = (T)Activator.CreateInstance(typeof(T), true);
                    }
                    return instance;
                }
            }
        }
    }
    /// <summary>
    /// 饿汉模式-单例模板
    /// </summary>
    public abstract class Singleton<T> where T : class, new()
    {
        private static readonly T instance = new T();
        public static T Instance
        {
            get
            {
                return instance;
            }
        }
    }
    /// <summary>
    /// Mono单例模板
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance = null;
        private static readonly object locker = new object();
        private static bool isAlive;
        public static T Instance
        {
            get
            {
                if (isAlive == false)
                {
                    instance = null;
                    return instance;
                }
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();
                        if (FindObjectsOfType<T>().Length > 1)
                        {
                            Debug.LogError($"不应该存在多个:{typeof(T)}单例！");
                            return instance;
                        }
                        if (instance == null)
                        {
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<T>();
                            singleton.name = "(Singleton)" + typeof(T);
                            singleton.hideFlags = HideFlags.None;
                            DontDestroyOnLoad(singleton);
                        }
                        else
                        {
                            DontDestroyOnLoad(instance.gameObject);
                        }
                    }
                    instance.hideFlags = HideFlags.None;
                    return instance;
                }
            }
        }
        protected virtual void Awake()
        {
            isAlive = true;
        }
        protected virtual void OnDestroy()
        {
            isAlive = false;
        }
    }
}