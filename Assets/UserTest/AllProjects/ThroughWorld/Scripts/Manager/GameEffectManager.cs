/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-08-28
*/

using System.Collections.Generic;
using UnityEngine;
using YangTools;
using YangTools.Extend;
using YangTools.ObjectPool;

public class GameEffectManager : MonoSingleton<GameEffectManager>
{
    /// <summary>
    /// 所有特效
    /// </summary>
    private static List<EffectObjectPoolItem> allEffect = new List<EffectObjectPoolItem>();

    /*
     * effectTypeId: 特效类型id，关联配置表相关项。
       controlPoints：特效相关控制点数据，后文详细讲述。
       controlEntities：特效相关单位控制数据，后文详细讲述。
       bRetain：EffectManager是否持有特效，后文详细讲述。
       effectHandle: 当bRetain=true时，创建特效后返回有效句柄，数值大于0，供游戏逻辑使用控制特效行为
       AttachType的定义如下：
       Attach_AbsOrigin: 特效基于目标坐标创建。
       Attach_AbsOrigin_Follow: 特效基于目标坐标创建并跟随目标位置移动。
       Attach_Point: 特效基于目标挂点(attachName)位置创建，但不跟随目标。
       Attach_Point_Follow: 特效基于目标挂点(attachName)位置创建，跟随目标。
     * */

    public EffectObjectPoolItem PlayEffect(string name, Vector3 worldPos)
    {
        EffectObjectPoolItem effectObjectPoolItem = GetEffet(null, worldPos, name);
        EffectAutoRecycle script = effectObjectPoolItem.obj.AddComponent<EffectAutoRecycle>();
        script.Init(effectObjectPoolItem);
        return effectObjectPoolItem;
    }

    /// <summary>
    /// 添加特效
    /// </summary>
    /// <param name="_target">跟随目标</param>
    /// <param name="name">名字</param>
    private EffectObjectPoolItem GetEffet(Transform _target, Vector3 worldPos, string name)
    {
        EffectData data = new EffectData(name);
        data.target = _target;
        data.worldPos = worldPos;
        return CreateEffet(data);
    }

    /// <summary>
    /// 创建特效
    /// </summary>
    private EffectObjectPoolItem CreateEffet(EffectData effectData)
    {
        //特效对象
        EffectObjectPoolItem poolItem = YangObjectPool.Get<EffectObjectPoolItem>(effectData.effectName, effectData.effectName);
        poolItem.InitData(effectData);
        GameObject effectObj = poolItem.obj;
        effectObj.transform.localPosition = effectData.worldPos;
        allEffect.Add(poolItem);

        return poolItem;
    }

    /// <summary>
    /// 回收特效
    /// </summary>
    public void RecycleEffet(EffectObjectPoolItem poolItem)
    {
        allEffect.Remove(poolItem);
        YangObjectPool.Recycle(poolItem);
    }
}

/// <summary>
/// 对象池特效对象
/// </summary>
public class EffectObjectPoolItem : IPoolItem<EffectObjectPoolItem>
{
    public string PoolKey { get; set; }
    public bool IsInPool { get; set; }
    public GameObject obj;

    public EffectObjectPoolItem()
    {
    }

    public EffectObjectPoolItem(string name)
    {
        GameObject tempObj = GameObject.Instantiate(GameResourceManager.Instance.ResoruceLoad($"Effects/{name}"));
        obj = tempObj;
    }

    public void InitData(EffectData effectData)
    {
    }

    public void OnGet()
    {
        obj.DefualtGameObjectOnGet(null);
    }

    public void OnRecycle()
    {
        obj.DefualtGameObjectRecycle();
    }

    public void OnDestroy()
    {
        obj.DefualtGameObjectDestory();
    }
}

/// <summary>
/// 特效信息
/// </summary>
public class EffectData
{
    public Transform target;
    public Vector3 worldPos;
    public string effectName;

    public EffectData(string _str)
    {
        effectName = _str;
    }
}