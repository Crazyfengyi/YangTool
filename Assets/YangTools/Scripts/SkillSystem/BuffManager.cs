/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuffManager
{
    #region 变量
    private readonly Dictionary<BuffType, List<BuffBase>> buffDic = new Dictionary<BuffType, List<BuffBase>>();
    #endregion

    #region 方法
    /// <summary>
    /// 获得指定类型的buff列表
    /// </summary>
    /// <param name="type">buff类型</param>
    /// <returns>可能返回null</returns>
    public List<BuffBase> GetBuffListOfType(BuffType type)
    {
        if (buffDic.ContainsKey(type))
        {
            return buffDic[type];
        }

        return null;
    }

    /// <summary>
    /// 获得是否有指定buff
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool GetHaveBuff(BuffType type)
    {
        return buffDic.ContainsKey(type);
    }

    /// <summary>
    /// 添加buff
    /// </summary>
    public void AddBuff(BuffBase buff)
    {
        //列表是否有这个类型buff
        if (buffDic.ContainsKey(buff.buffTypeId))
        {
            if (buff.OnBuffAwake(this, true))
            {

                if (buff.OnBuffRefresh(this, buff))
                {
                    buffDic[buff.buffTypeId].Add(buff);
                }

                //TODO:不管怎么处理都调start
                buff.OnBuffStart();
            }
        }
        else
        {
            if (buff.OnBuffAwake(this, false))
            {
                buffDic.Add(buff.buffTypeId, new List<BuffBase>() { buff });
                buff.OnBuffStart();
            }
        }


    }

    /// <summary>
    /// 移除buff
    /// </summary>
    public bool RemoveBuff(BuffType type)
    {
        if (buffDic.ContainsKey(type))
        {
            List<BuffBase> list = buffDic[type];

            for (int i = 0; i < list.Count; i++)
            {
                list[i].OnBuffRemove();
            }

            buffDic[type] = null;
            buffDic.Remove(type);

            for (int i = 0; i < list.Count; i++)
            {
                list[i].OnBuffDestroy();
            }
            return true;
        }

        return false;
    }
    #endregion
}