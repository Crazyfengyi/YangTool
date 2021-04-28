/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using UnityEngine;
using System.Collections;


[CreateAssetMenu(fileName = "AttributeBuff", menuName = "BuffSystem/AttributeBuff")]
public class AttributeBuff : BuffBase
{
    #region 变量
    public int value;
    #endregion

    #region 生命周期

    #endregion

    #region 方法
    #endregion
    public override void Init(object _value)
    {
        value = (int)_value;
    }
}