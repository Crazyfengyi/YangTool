/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2021-12-23
 */

using UnityEngine;

namespace YangTools.Scripts.Core
{
    /// <summary>
    /// 饿汉模式-单例模板
    /// </summary>
    public abstract class Singleton<T> where T : class, new()
    {
        public static T Instance { get; } = new T();
    }

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
                        //支持非公共的无参构造函数，new T()不支持非公共的无参构造函数
                        //instance = (T)Activator.CreateInstance(typeof(T), true);
                    }

                    return instance;
                }
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
        private static bool isInstanceDestory;
        public static bool IsInit => instance = null;
        public static T Instance
        {
            get
            {
                if (isInstanceDestory)
                {
                    instance = null;
                    return instance;
                }

                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = FindFirstObjectByType<T>();
                        if (FindObjectsByType<T>(FindObjectsSortMode.InstanceID).Length > 1)
                        {
                            Debug.LogError($"不应该存在多个{typeof(T)}单例！");
                            return instance;
                        }

                        if (instance == null)
                        {
                            Debug.Log($"初始化{typeof(T)}");
                            GameObject singleton = new GameObject();
                            instance = singleton.AddComponent<T>();
                            singleton.name = typeof(T) + "(Singleton)";
                            singleton.hideFlags = HideFlags.None;
                            DontDestroyOnLoad(singleton);
                        }
                        else
                        {
                            if (Application.isPlaying)
                            {
                                instance.hideFlags = HideFlags.None;
                                DontDestroyOnLoad(instance.gameObject);
                            }
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            isInstanceDestory = false;
        }

        protected virtual void OnDestroy()
        {
            isInstanceDestory = true;
        }
    }

    /// <summary>
    /// Mono注册性单例
    /// </summary>
    /// public class Example : MonoBehaviour
    /// {
    ///     public static Example Instance => MonoSingletonRegister<Example>.Instance;
    /// }
    public static class MonoSingletonRegister<T> where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object locker = new object();

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            GameObject gameObject = new GameObject(typeof(T).Name + "(SingletonRegister)");
                            Object.DontDestroyOnLoad(gameObject);
                            instance = gameObject.AddComponent<T>();
                        }
                    }
                }

                return instance;
            }
        }
    }
}