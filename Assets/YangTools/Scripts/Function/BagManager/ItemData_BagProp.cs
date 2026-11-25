using System;
using System.Collections.Generic;
using cfg;
using cfg.item;
using UnityEngine;

/// <summary>
/// 保存运行时道具标识和数量并提供数量运算
/// </summary>
public class ItemData_BagProp
{
    public Item Config { get; private set; }
    public int PropId => Config.Id;
    public float PropCount { get; private set; }

    public string PropCountString
    {
        get
        {
            return PropCount.ToString("0");
        }
    }

    /// <summary>
    /// 使用道具标识与初始数量创建运行时数据
    /// </summary>
    /// <param name="propId">道具标识</param>
    /// <param name="propCount">本次使用的道具数量</param>
    public ItemData_BagProp(int propId, float propCount)
    {
        Config = GameTableManager.Instance.Tables.TbItem.GetOrDefault(propId);
        if (Config == null)
        {
            Debug.LogError($"不存在的道具:{propId}");
        }

        PropCount = propCount;
    }

    /// <summary>
    /// 判断当前数量是否满足所需数量
    /// </summary>
    /// <param name="propCount">本次使用的道具数量</param>
    /// <returns>数量足够时返回 true</returns>
    public bool BagPropEnough(int propCount)
    {
        return PropCount >= propCount;
    }

    /// <summary>
    /// 累加道具数量
    /// </summary>
    /// <param name="addCount">增加的道具数量</param>
    public void AddProp(float addCount)
    {
        PropCount += addCount;
    }

    /// <summary>
    /// 设置道具的绝对数量
    /// </summary>
    /// <param name="bagPropCount">设置后的道具数量</param>
    public void SetBagProp(float bagPropCount)
    {
        PropCount = bagPropCount;
    }

    /// <summary>
    /// 直接扣减道具数量 调用方负责校验余额
    /// </summary>
    /// <param name="removeCount">扣除的道具数量</param>
    public void RemoveProp(float removeCount)
    {
        PropCount -= removeCount;
    }

    /// <summary>
    /// 将道具数量覆盖为指定值
    /// </summary>
    /// <param name="propCount">本次使用的道具数量</param>
    public void EqualProp(float propCount)
    {
        PropCount = propCount;
    }
}