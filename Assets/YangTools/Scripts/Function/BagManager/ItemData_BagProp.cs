using System;
using System.Collections.Generic;
using cfg;
using cfg.item;
using UnityEngine;

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

    public ItemData_BagProp(int propId, float propCount)
    {
        Config = GameTableManager.Instance.Tables.TbItem.GetOrDefault(propId);
        if (Config == null)
        {
            Debug.LogError($"不存在的道具:{propId}");
        }

        PropCount = propCount;
    }

    public bool BagPropEnough(int propCount)
    {
        return PropCount >= propCount;
    }

    public void AddProp(float addCount)
    {
        PropCount += addCount;
    }

    public void SetBagProp(float bagPropCount)
    {
        PropCount = bagPropCount;
    }

    public void RemoveProp(float removeCount)
    {
        PropCount -= removeCount;
    }

    public void EqualProp(float propCount)
    {
        PropCount = propCount;
    }
}