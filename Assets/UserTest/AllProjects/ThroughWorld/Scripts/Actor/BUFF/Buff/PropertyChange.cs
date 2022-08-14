/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.1f1c1 
 *创建时间:         2021-04-27 
*/
using UnityEngine;
using System.Collections;

/// <summary>
/// 属性更改
/// </summary>
public class PropertyChange : BuffBase
{
    public int value;
    public PropertyChange(RoleBase _creator, int configId) : base(_creator, configId)
    {
    }
    public override void Init(BuffConfig buffConfig)
    {
        base.Init(buffConfig);
    }
}