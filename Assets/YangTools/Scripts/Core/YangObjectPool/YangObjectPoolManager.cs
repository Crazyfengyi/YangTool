/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-11-11 
*/
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine.Pool;
using static UnityEditor.Progress;

namespace YangTools.ObjectPool
{
    /// <summary>
    /// 通用对象池
    /// </summary>
    public class YangObjectPool
    {
        /// <summary>
        /// 是否检测回收对象(是否允许没有从对象池取出直接调用放入)
        /// </summary>
        public static bool isCheckRelease = true;
        //所有对象池
        private static readonly Dictionary<string, PoolPackage> m_allPools = new Dictionary<string, PoolPackage>();
        /// <summary>
        /// 获得对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象</returns>
        public static T Get<T>() where T : class, IPoolItem<T>, new()
        {
            string key = typeof(T).FullName;
            if (m_allPools.ContainsKey(key))
            {
                return m_allPools[key].GetPool<T>().Get();
            }
            else
            {
                ObjectPool<T> pool = new ObjectPool<T>();
                PoolPackage package = new PoolPackage();
                package.objectPool = pool;
                m_allPools.Add(key, package);
                return m_allPools[key].GetPool<T>().Get();
            }
        }
        /// <summary>
        /// 回收对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="item">要回收的对象</param>
        /// <returns>是否回收成功</returns> 
        public static bool Release<T>(T item) where T : class, IPoolItem<T>, new()
        {
            string key = typeof(T).FullName;
            if (m_allPools.ContainsKey(key))
            {
                m_allPools[key].GetPool<T>().Release(item);
                return true;
            }
            else
            {
                if (isCheckRelease)
                {
                    ObjectPool<T> pool = new ObjectPool<T>();
                    PoolPackage package = new PoolPackage();
                    package.objectPool = pool;
                    m_allPools.Add(key, package);
                    m_allPools[key].GetPool<T>().Release(item);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 清空对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>是否清空成功</returns>
        public static bool Clear<T>() where T : class, IPoolItem<T>, new()
        {
            string key = typeof(T).FullName;
            if (m_allPools.ContainsKey(key))
            {
                m_allPools[key].GetPool<T>().Clear();
                return true;
            }

            return false;
        }
        /// <summary>
        /// 是否有泛型接口
        /// </summary>
        private static bool HaveInterface(Type type)
        {
            return Array.Exists(type.GetInterfaces(), t =>
            {
                if (t.IsGenericType)
                {
                    return t.GetGenericTypeDefinition() == typeof(IPoolItem<>);
                }

                return false;
            });
        }
    }
    /// <summary>
    /// 对象池包裹类
    /// </summary>
    public class PoolPackage
    {
        public dynamic objectPool;
        public ObjectPool<T> GetPool<T>() where T : class, IPoolItem<T>, new()
        {
            return (ObjectPool<T>)(object)objectPool;
        }
    }
    public class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class, IPoolItem<T>, new()
    {
        internal readonly Stack<T> m_Stack;//栈
        private readonly ReflectionInfo m_CreateFunc;//创建方法
        private readonly ReflectionInfo m_ActionOnGet;//方法时调用
        private readonly ReflectionInfo m_ActionOnRelease;//回收时调用
        private readonly ReflectionInfo m_ActionOnDestroy;//删除时调用

        private readonly int m_MaxSize;//最大数量
        internal bool m_CollectionCheck;//回收检查(防止将已经在对象池的对象重复放进对象池)
        /// <summary>
        /// 所有对象数
        /// </summary>
        public int CountAll
        {
            get;
            private set;
        }
        /// <summary>
        /// 使用数量
        /// </summary>
        public int CountActive => CountAll - CountInactive;
        /// <summary>
        /// 未使用数量
        /// </summary>
        public int CountInactive => m_Stack.Count;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="collectionCheck">是否回收检查</param>
        /// <param name="defaultCapacity">默认容量</param>
        /// <param name="maxSize">最大大小</param>
        public ObjectPool(int defaultCapacity = 10, int maxSize = 10000, bool collectionCheck = true)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }
            m_Stack = new Stack<T>(defaultCapacity);
            m_MaxSize = maxSize;
            m_CollectionCheck = collectionCheck;

            #region 反射获取方法
            Type type = typeof(T);

            FieldInfo inter = null;
            Type[] interNames = type.GetInterfaces();
            for (int i = 0; i < interNames.Length; i++)
            {
                Type item = interNames[i];
                if (item.Name.Contains("IPoolItem"))
                {
                    inter = item.GetField("instanceT", BindingFlags.Static | BindingFlags.NonPublic);
                    break;
                }
            }

            object instance = inter.GetValue(type);
            System.Reflection.MethodInfo createFunc = type.GetMethod("Create");
            System.Reflection.MethodInfo getAction = type.GetMethod("OnGet");
            System.Reflection.MethodInfo releaseAction = type.GetMethod("OnRelease");
            System.Reflection.MethodInfo destroyAction = type.GetMethod("OnDestroy");

            m_CreateFunc = new ReflectionInfo(instance, createFunc);
            m_ActionOnGet = new ReflectionInfo(instance, getAction);
            m_ActionOnRelease = new ReflectionInfo(instance, releaseAction);
            m_ActionOnDestroy = new ReflectionInfo(instance, destroyAction);
            #endregion
        }

        public T Get()
        {
            T item;
            if (m_Stack.Count == 0)
            {
                item = m_CreateFunc?.Invoke<T>(true);
                CountAll++;
            }
            else
            {
                item = m_Stack.Pop();
            }

            item.IsInPool = false;
            m_ActionOnGet?.Invoke<T>(false, item);
            return item;
        }
        public PooledObjectPackage<T> GetAutoReleaseItem()
        {
            return new PooledObjectPackage<T>(Get(), this);
        }
        public void Release(T item)
        {
            if (m_CollectionCheck && m_Stack.Count > 0 && m_Stack.Contains(item))
            {
                throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
            }

            m_ActionOnRelease?.Invoke<T>(false, item);
            if (CountInactive < m_MaxSize)
            {
                item.IsInPool = true;
                m_Stack.Push(item);
            }
            else
            {
                m_ActionOnDestroy?.Invoke<T>(false, item);
            }
        }
        public void Clear()
        {
            foreach (T item in m_Stack)
            {
                item.IsInPool = false;
                m_ActionOnDestroy?.Invoke<T>(false, item);
            }
            m_Stack.Clear();
            CountAll = 0;
        }
        public void Dispose()
        {
            Clear();
        }
    }
    /// <summary>
    /// 反射信息
    /// </summary>
    public class ReflectionInfo
    {
        object instanceT;
        System.Reflection.MethodInfo Func;
        public ReflectionInfo(object _instanceT, System.Reflection.MethodInfo _createFun)
        {
            instanceT = _instanceT;
            Func = _createFun;
        }
        public T Invoke<T>(bool isNull, params object[] paramlist)
        {
            if (isNull)
            {
                return (T)Func.Invoke(instanceT, null);
            }
            else
            {
                //反射调用
                return (T)Func.Invoke(instanceT, new object[] { paramlist[0] });
            }
        }
    }
    /// <summary>
    /// 对象池
    /// </summary>
    /// <typeparam name="T">对象</typeparam>
    public interface IObjectPool<T> where T : class
    {
        /// <summary>
        /// 不活跃的数量
        /// </summary>
        int CountInactive
        {
            get;
        }
        T Get();
        PooledObjectPackage<T> GetAutoReleaseItem();
        void Release(T element);
        void Clear();
    }
    // C# 8.0 可在接口中实现static方法 和 构造函数--可以改的更好--https://zhuanlan.zhihu.com/p/268278929
    /// <summary>
    /// 要放进对象池的对象
    /// </summary>
    public interface IPoolItem<T> where T : new()
    {
        bool IsInPool { get; set; }
        protected static T instanceT = new T(); /* (T)Activator.CreateInstance(typeof(T))*/
        T Create();
        void OnGet(T t);
        void OnRelease(T t);
        void OnDestroy(T t);
    }
    /// <summary>
    /// 值类型-对象池物体包裹-值类型回收时(销毁时)自动回收到对象池
    /// </summary>
    /// <typeparam name="T">对象</typeparam>
    public struct PooledObjectPackage<T> : IDisposable where T : class
    {
        private readonly T m_ToReturn;
        private readonly IObjectPool<T> m_Pool;
        internal PooledObjectPackage(T value, IObjectPool<T> pool)
        {
            m_ToReturn = value;
            m_Pool = pool;
        }
        public T GetData()
        {
            return m_ToReturn;
        }
        void IDisposable.Dispose()
        {
            m_Pool.Release(m_ToReturn);
        }
    }
    /// <summary>
    /// 默认对象池对象
    /// </summary>
    public class DefaultObjectPoolItem : IPoolItem<DefaultObjectPoolItem>
    {
        public bool IsInPool { get; set; }
        public DefaultObjectPoolItem Create()
        {
            return new DefaultObjectPoolItem();
        }
        public void OnGet(DefaultObjectPoolItem t)
        {
        }
        public void OnRelease(DefaultObjectPoolItem t)
        {
        }
        public void OnDestroy(DefaultObjectPoolItem t)
        {
        }
    }
}