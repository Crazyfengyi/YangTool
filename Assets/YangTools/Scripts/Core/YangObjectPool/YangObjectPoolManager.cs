/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       YangWork
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2021-11-11
*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace YangTools.ObjectPool
{
    #region 对象池

    /// <summary>
    /// 通用对象池
    /// </summary>
    public class YangObjectPool : GameModuleBase
    {
        /// <summary>
        /// 是否检测回收对象(是否允许没有从对象池取出直接调用放入)
        /// </summary>
        public static bool IsCheckRecyle { get; set; } = true;

        //所有对象池
        private static readonly Dictionary<string, PoolPackage> m_allPools = new Dictionary<string, PoolPackage>();

        #region 生命周期

        internal override void InitModule()
        {
        }

        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            foreach (KeyValuePair<string, PoolPackage> item in m_allPools)
            {
                item.Value.Update(delaTimeSeconds, unscaledDeltaTimeSeconds);
            }
        }

        internal override void CloseModule()
        {
            m_allPools.Clear();
        }

        #endregion 生命周期

        #region 静态方法

        /// <summary>
        /// 获得对象
        /// </summary>
        /// <param name="poolKey">对象池key,不传默认用脚本类名</param>
        public static T Get<T>(string poolKey = "", params object[] args) where T : class, IPoolItem<T>, new()
        {
            string resultKey = GetTypeKey<T>(poolKey);

            if (m_allPools.ContainsKey(resultKey))
            {
                return m_allPools[resultKey].GetPool<T>().Get(args);
            }
            else
            {
                CreatePool<T>(resultKey);
                return m_allPools[resultKey].GetPool<T>().Get(args);
            }
        }

        /// <summary>
        /// 获得自动回收包裹
        /// </summary>
        public static PooledObjectPackage<T> Get<T>(out T item, string poolKey = "") where T : class, IPoolItem<T>, new()
        {
            item = null;
            string resultKey = GetTypeKey<T>(poolKey);

            if (m_allPools.ContainsKey(resultKey))
            {
                return m_allPools[resultKey].GetPool<T>().GetAutoRecycleItem();
            }
            else
            {
                CreatePool<T>(resultKey);
                return m_allPools[resultKey].GetPool<T>().GetAutoRecycleItem();
            }
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        public static void CreatePool<T>(string key) where T : class, IPoolItem<T>, new()
        {
            ObjectPool<T> pool = new ObjectPool<T>();
            pool.PoolKey = key;

            //包裹类-为了将对象池放进字典统一管理
            PoolPackage package = new PoolPackage();
            package.objectPool = pool;
            package.T_type = typeof(T);
            package.Pool_type = pool.GetType();
            package.Binding<T>();

            m_allPools.Add(key, package);
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <returns>是否回收成功</returns>
        public static bool Recycle<T>(T item) where T : class, IPoolItem<T>, new()
        {
            if (item == null) return false;
            string key = item.PoolKey;
            if (string.IsNullOrEmpty(key))
            {
                key = typeof(T).FullName;
            }
            if (m_allPools.ContainsKey(key))
            {
                m_allPools[key].GetPool<T>().Recycle(item);
                return true;
            }
            else
            {
                if (IsCheckRecyle)
                {
                    ObjectPool<T> pool = new ObjectPool<T>();
                    pool.PoolKey = item.PoolKey;
                    PoolPackage package = new PoolPackage();
                    package.objectPool = pool;
                    package.T_type = item.GetType();
                    package.Pool_type = pool.GetType();
                    package.Binding<T>();
                    m_allPools.Add(key, package);
                    m_allPools[key].GetPool<T>().Recycle(item);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
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
        /// 获得类型的对象池Key
        /// </summary>
        public static string GetTypeKey<T>(string poolKey = "")
        {
            //默认用全路径
            string resultKey = typeof(T).FullName;
            //如果有自定义名称
            if (!string.IsNullOrEmpty(poolKey))
            {
                resultKey = poolKey;
            }

            return resultKey;
        }

        #endregion 静态方法
    }

    #region 对象池接口和类

    /// <summary>
    /// 对象池包裹类-将对象池放进字典统一管理
    /// </summary>
    internal class PoolPackage
    {
        public object objectPool;
        public Type Pool_type;
        public Type T_type;
        private ReflectionInfo updateAction;//轮询方法

        public ObjectPool<T> GetPool<T>() where T : class, IPoolItem<T>, new()
        {
            return (ObjectPool<T>)(object)objectPool;
        }

        /// <summary>
        /// 绑定对象池的轮询方法
        /// </summary>
        public void Binding<T>()
        {
            Type type = Pool_type;
            System.Reflection.MethodInfo createFunc = type.GetMethod("Update");
            updateAction = new ReflectionInfo(objectPool, createFunc);
        }

        public void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            updateAction?.Invoke(delaTimeSeconds, unscaledDeltaTimeSeconds);
        }
    }

    /// <summary>
    /// 对象池
    /// </summary>
    public class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class, IPoolItem<T>, new()
    {
        public string PoolKey { get; set; }
        internal readonly Stack<T> m_Stack;//栈
        private readonly MethodInfo createFunc;//创建方法
        private readonly MethodInfo createFunc2;//有参创建方法

        private readonly int m_MaxSize;//最大数量
        internal bool m_CollectionCheck;//回收检查(防止将已经在对象池的对象重复放进对象池)

        internal int m_defaultCapacity;//默认对象池大小

        /// <summary>
        /// 所有对象数
        /// </summary>
        public int AllCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 使用数量
        /// </summary>
        public int ActiveCount => AllCount - InactiveCount;

        /// <summary>
        /// 未使用数量
        /// </summary>
        public int InactiveCount => m_Stack.Count;

        /// <summary>
        /// 自动回收间隔时间
        /// </summary>
        public float AutoRecycleInterval { get; set; }

        /// <summary>
        /// 自动回收计时
        /// </summary>
        public float AutoRecycleTime { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public float Priority { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="defaultCapacity">默认容量</param>
        /// <param name="maxSize">最大大小</param>
        /// <param name="autoRecycleTime">自动回收间隔时间</param>
        /// <param name="priority">优先级</param>
        /// <param name="collectionCheck">是否回收检查</param>
        public ObjectPool(int defaultCapacity = 10, int maxSize = 10000, float autoRecycleTime = 60, float priority = 0, bool collectionCheck = true)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", "maxSize");
            }
            m_Stack = new Stack<T>(defaultCapacity);
            m_defaultCapacity = defaultCapacity;
            m_MaxSize = maxSize;
            AutoRecycleInterval = autoRecycleTime;
            AutoRecycleTime = AutoRecycleInterval;
            Priority = priority;
            m_CollectionCheck = collectionCheck;

            #region 反射获取方法

            Type type = typeof(T);

            //反射拿到静态方法PoolCreate
            MethodInfo methodInfoCreate = null;
            MethodInfo methodInfoCreate2 = null;

            Type[] interNames = type.GetInterfaces();
            for (int i = 0; i < interNames.Length; i++)
            {
                Type item = interNames[i];
                if (item.Name.Contains("IPoolItem"))
                {
                    methodInfoCreate = item.GetMethod("PoolCreate", new Type[0]);
                    methodInfoCreate2 = item.GetMethod("PoolCreate", new Type[] { typeof(object[]) });
                    break;
                }
            }

            createFunc = methodInfoCreate;
            createFunc2 = methodInfoCreate2;

            #endregion 反射获取方法
        }

        public void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            AutoRecycleTime += unscaledDeltaTimeSeconds;
            if (AutoRecycleTime < AutoRecycleInterval)
            {
                return;
            }
            RecycleToDefaultCount();
        }

        public T Get(params object[] args)
        {
            T item;
            if (m_Stack.Count == 0)
            {
                if (args.Length > 0)
                {
                    if (createFunc2 == null)
                    {
                        throw new InvalidOperationException();
                    }
                    item = (T)createFunc2?.Invoke(null, new object[] { args });
                }
                else
                {
                    item = (T)createFunc?.Invoke(null, null);
                }
                item.PoolKey = PoolKey;
                AllCount++;
            }
            else
            {
                item = m_Stack.Pop();
            }
            item.IsInPool = false;
            item.OnGet();
            return item;
        }

        public PooledObjectPackage<T> GetAutoRecycleItem()
        {
            return new PooledObjectPackage<T>(Get(), this);
        }

        public void RecycleToDefaultCount()
        {
            AutoRecycleTime = 0f;
            while (m_Stack.Count > m_defaultCapacity)
            {
                T item = m_Stack.Peek();
                Recycle(item);
            }
        }

        public void Recycle(T item)
        {
            if (m_CollectionCheck && m_Stack.Count > 0 && m_Stack.Contains(item))
            {
                throw new InvalidOperationException("试图释放一个已经释放到池中的对象.");
            }

            item.OnRecycle();
            if (InactiveCount < m_MaxSize)
            {
                item.IsInPool = true;
                m_Stack.Push(item);
            }
            else
            {
                item.OnDestroy();
            }
        }

        public void Clear()
        {
            foreach (T item in m_Stack)
            {
                item.IsInPool = false;
                item.OnDestroy();
            }
            m_Stack.Clear();
            AllCount = 0;
        }

        public void Dispose()
        {
            Clear();
        }
    }

    /// <summary>
    /// 对象池
    /// </summary>
    public interface IObjectPool<T> where T : class
    {
        /// <summary>
        /// 对象池在管理类字典里的key
        /// </summary>
        string PoolKey { get; set; }

        /// <summary>
        /// 不活跃的数量
        /// </summary>
        int InactiveCount
        {
            get;
        }

        /// <summary>
        /// 自动回收间隔
        /// </summary>
        float AutoRecycleInterval { get; set; }

        /// <summary>
        /// 自动回收计时
        /// </summary>
        float AutoRecycleTime { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        float Priority { get; set; }

        void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds);

        /// <summary>
        /// 获得对象
        /// </summary>
        T Get(params object[] args);

        /// <summary>
        /// 获得自动回收包裹
        /// </summary>
        PooledObjectPackage<T> GetAutoRecycleItem();

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="element"></param>
        void Recycle(T element);

        /// <summary>
        /// 释放对象池中的对象到默认大小
        /// </summary>
        void RecycleToDefaultCount();

        /// <summary>
        /// 清空对象池
        /// </summary>
        void Clear();
    }

    /*
     * 使用事例(Unity官方的):
        var simplifiedPoints = CollectionPool<List<Vector2>, Vector2>.Get();
        using (CollectionPool<List<Vector2>, Vector2>.Get(out List<Vector2> tempList))
        {
            for (int i = 0; i < points.Length; ++i)
            {
                tempList.Add(points[i]);
            }

            LineUtility.Simplify(tempList, 1.5f, simplifiedPoints);
        }
        return simplifiedPoints;
     * */

    //值类型-值类型回收时(销毁时)自动回收对象到对象池--Unity默认对象池抄的
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
            m_Pool.Recycle(m_ToReturn);
        }
    }

    /// <summary>
    /// 对象池物体接口
    /// </summary>
    public interface IPoolItem<T> where T : new()
    {
        /// <summary>
        /// 所属对象池在管理类字典里的key
        /// </summary>
        string PoolKey { get; set; }

        bool IsInPool { get; set; }

        public static T PoolCreate()
        {
            T result = new T();//在构造函数里初始化
            return result;
        }

        public static T PoolCreate(params object[] args)
        {
            T result = (T)Activator.CreateInstance(typeof(T), args);//在构造函数里初始化
            return result;
        }

        /// <summary>
        /// 获得时
        /// </summary>
        void OnGet();

        /// <summary>
        /// 回收时
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// 删除时
        /// </summary>
        void OnDestroy();
    }

    /// <summary>
    /// 反射信息
    /// </summary>
    public class ReflectionInfo
    {
        private object instanceT;
        private System.Reflection.MethodInfo Func;

        public ReflectionInfo(object _instanceT, System.Reflection.MethodInfo _createFun)
        {
            instanceT = _instanceT;
            Func = _createFun;
        }

        public T Invoke<T>(bool isNull, params object[] paramlist)
        {
            if (isNull)
            {
                return (T)Func?.Invoke(instanceT, null);
            }
            else
            {
                //反射调用
                return (T)Func?.Invoke(instanceT, new object[] { paramlist[0] });
            }
        }

        /// <summary>
        /// 无返回值
        /// </summary>
        /// <param name="paramlist">参数列表</param>
        public void Invoke(params object[] paramlist)
        {
            bool isNull = paramlist.Length == 0;
            if (isNull)
            {
                Func.Invoke(instanceT, null);
            }
            else
            {
                //反射调用
                Func.Invoke(instanceT, paramlist);
            }
        }
    }

    #endregion 对象池接口和类

    /// <summary>
    /// 默认对象池对象
    /// </summary>
    public class DefaultObjectPoolItem : IPoolItem<DefaultObjectPoolItem>
    {
        public string PoolKey { get; set; }
        public bool IsInPool { get; set; }

        public DefaultObjectPoolItem()
        {
        }

        public void OnGet()
        {
        }

        public void OnRecycle()
        {
        }

        public void OnDestroy()
        {
        }
    }

    #endregion 对象池

    #region 引用池

    /// <summary>
    /// 引用池
    /// </summary>
    public static class ReferencePool
    {
        private static Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();
        private static readonly Object lockObject = new Object();

        /// <summary>
        /// 获取
        /// </summary>
        public static T Get<T>()
        {
            T newObject;
            lock (lockObject)
            {
                if (poolDictionary.ContainsKey(typeof(T)))
                {
                    var pooledObjects = poolDictionary[typeof(T)] as Stack<T>;
                    if (pooledObjects.Count > 0)
                    {
                        return pooledObjects.Pop();
                    }
                }

                newObject = (T)CreateInstance(typeof(T));
            }

            return newObject;
        }

        /// <summary>
        /// 回收
        /// </summary>
        public static void Recycle<T>(T obj)
        {
            if (obj == null)
            {
                return;
            }

            lock (lockObject)
            {
                object value;
                if (poolDictionary.TryGetValue(typeof(T), out value))
                {
                    var pooledObjects = value as Stack<T>;
                    pooledObjects.Push(obj);
                }
                else
                {
                    var pooledObjects = new Stack<T>();
                    pooledObjects.Push(obj);
                    poolDictionary.Add(typeof(T), pooledObjects);
                }
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public static void Clear()
        {
            lock (lockObject)
            {
                poolDictionary.Clear();
            }
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        public static object CreateInstance(Type t)
        {
#if NETFX_CORE && !UNITY_EDITOR//微软VR
            if (t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
#else
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
#endif
                t = Nullable.GetUnderlyingType(t);
            }
#if NETFX_CORE && !UNITY_EDITOR//微软VR
            return Activator.CreateInstance(t);
#else
            return Activator.CreateInstance(t, true);
#endif
        }
    }

    #endregion 引用池
}