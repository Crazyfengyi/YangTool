#define USE_HASHCODE //当用HashCode时从对象池取出对象后可以改名

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YangTools;

/// 使用时可用示例,使用unity设置面板进行相关选项(未实现)
/// <summary>
/// 对象池
/// </summary> //sealed(密封),不可以派生重写
public sealed class YangToolObjectPool
{
    #region 单例
    /// <summary>
    /// 单例
    /// </summary>
    private static YangToolObjectPool instance;
    /// <summary>
    /// 设备线程锁
    /// </summary>
    private static readonly object padlock = new object();
    /// <summary>
    /// 单例
    /// </summary>
    public static YangToolObjectPool Instance
    {
        get
        {
            ///双重锁定，先判断实例是否存在，不存在再加锁处理
            ///这样不用让线程每次都加锁，保证了线程安全，也提高了性能
            if (instance == null)
            {
                lock (padlock)
                {
                    if (Instance == null)
                    {
                        instance = new YangToolObjectPool();
                    }
                    return instance;
                }
            }
            return instance;
        }
    }
    /// <summary>
    /// 初始化列表
    /// </summary>
    public static Dictionary<string, int> initDict = new Dictionary<string, int>();
    /// <summary>
    /// 对象池
    /// </summary>
    private static Dictionary<string, List<GameObject>> GamePool = new Dictionary<string, List<GameObject>>();
    /// <summary>
    /// 预制物体池
    /// </summary>
    private static Dictionary<string, GameObject> GameObjectPrefabs = new Dictionary<string, GameObject>();

    #endregion

    /// <summary>
    /// 构造函数，为保证顺序必须有
    /// </summary>
    static YangToolObjectPool()
    {
        //TODO 需要在这里通过表对initDict进行初始化
    }

    /// <summary>
    /// 初始化,实现连续的内存，提示在指定位置调用（游戏开始动画/过场白屏）--会卡（循环生成物体）
    /// </summary>
    public static void Init()
    {
        foreach (var item in initDict)
        {
            InitPool(item.Key, item.Value);
        }
    }

    /// <summary>
    /// 初始化使用，生成对应数量物体放入对象池
    /// </summary>
    /// <param name="objName">物体名字</param>
    /// <param name="count">数量</param>
    public static void InitPool(string objName, int count)
    {
        GameObject prefab = null;
        //如果已经加载过预制体
        if (GameObjectPrefabs.ContainsKey(objName))
        {
            prefab = GameObjectPrefabs[objName];
        }
        else
        {
            try
            {
                prefab = Resources.Load<GameObject>("Prefabs/" + objName);
                GameObjectPrefabs.Add(objName, prefab);
            }
            catch
            {
                Debug.LogError($"Resources not have {objName}");
                return;
            }
        }

        for (int i = 0; i < count; i++)
        {
            //生成物体
            GameObject result = UnityEngine.Object.Instantiate(prefab);
            result.name = objName;
            //放进对象池
            RecycleObject(result);
        }
    }

    #region 对象池对外方法
    /// <summary>
    /// 获得对象从对象池，如果对象池没有就从Resources加载
    /// </summary>
    /// <param name="ObjectName">物体名字(不要更改，对象池通过名字管理)</param>
    /// <returns></returns>
    public GameObject GetObjectFromPool(string ObjectName)
    {
        GameObject result = null;

        //是否有该名字的对象池
        if (GamePool.ContainsKey(ObjectName))
        {
            //对象池是否空的
            if (GamePool[ObjectName].Count != 0)
            {
                //得到物体
                result = GamePool[ObjectName][0];
                result.SetActive(true);
                GamePool[ObjectName].Remove(result);
                return result;
            }
        }

        //如果没有该名字的对象池或对象池是空的
        GameObject prefab = null;
        //如果已经加载过预制体
        if (GameObjectPrefabs.ContainsKey(ObjectName))
        {
            prefab = GameObjectPrefabs[ObjectName];
        }
        else
        {
            try
            {
                prefab = Resources.Load<GameObject>("Prefabs/" + ObjectName);
                GameObjectPrefabs.Add(ObjectName, prefab);
            }
            catch
            {
                Debug.LogError($"Resources not have {ObjectName}");
            }
        }
        //生成物体
        result = UnityEngine.Object.Instantiate(prefab);
        result.name = ObjectName;
#if USE_HASHCODE
        result.SetExtendName();
#endif
        return result;
    }

    /// <summary>
    /// 把对象放回对象池
    /// </summary>
    /// <param name="obj"></param>
    public static void RecycleObject(GameObject obj)
    {
        obj.SetActive(false);
#if USE_HASHCODE
        string extendName = obj.GetExtendName();

        if (extendName.IsNull())
        {
            Debug.LogError($"{obj.name} not have {extendName}");
            return;
        }
        //是否有该对象池
        if (GamePool.ContainsKey(extendName))
        {
            GamePool[extendName].Add(obj);
        }
        else
        {
            //创建对象池，并放入物体
            GamePool.Add(extendName, new List<GameObject>() { obj });
        }
#else
        //是否有该对象池
        if (GamePool.ContainsKey(obj.name))
        {
            GamePool[obj.name].Add(obj);
        }
        else
        {
            //创建对象池，并放入物体
            GamePool.Add(obj.name, new List<GameObject>() { obj });
        }
#endif
    }

    /// <summary>
    /// 清空指定对象池 
    /// </summary>
    /// <param name="poolName"></param>
    public void ClearGamePool(string poolName)
    {
        // 需要一个清空指定对象池的函数
        if (GamePool.ContainsKey(poolName))
        {
            //删除物体
            for (int i = GamePool[poolName].Count; i >= 0; i--)
            {
                UnityEngine.GameObject.Destroy(GamePool[poolName][i]);
            }
            GamePool[poolName] = null;
        }
    }

    /// <summary>
    /// 判断物体是否在对象池中，可以直接调用GameObject.IsInGamePool();
    /// 只有用USE_HASHCODE时可用，不然返回Null
    /// </summary>
    /// <param name="obj">物体</param>
    /// <returns></returns>
    public bool? IsInGamePool(GameObject obj)
    {
#if USE_HASHCODE
        return obj.IsInGamePool();
#else
        return null;
#endif
    }
    #endregion

}
