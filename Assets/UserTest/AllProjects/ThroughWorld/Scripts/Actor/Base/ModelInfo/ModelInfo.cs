/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-09-09 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using System.Collections.Generic;

/// <summary>
/// 模型信息
/// </summary>
public class ModelInfo : MonoBehaviour
{
    /// <summary>
    /// 子弹发射点
    /// </summary>
    public List<Transform> ShootPoint = new List<Transform>();
    public Transform Root;
    public Transform Head;
    public Transform LeftHand;
    public Transform RightHand;
    public Transform Body;
    [HideInInspector]
    public Transform Foot;

    public void Awake()
    {
        Root = transform;
        Foot = transform;
        //TODO:根据规则绑定,或者序列化
        Head = transform;
        LeftHand = transform;
        RightHand = transform;
        Body = transform;
    }
    /// <summary>
    /// 获得特效点
    /// </summary>
    public Transform GetEffectPoint(ModelPointType effectPointType)
    {
        switch (effectPointType)
        {
            case ModelPointType.Root:
                return Root;
            case ModelPointType.Foot:
                return Foot;
            case ModelPointType.Head:
                return Head;
            case ModelPointType.Body:
                return Body;
            case ModelPointType.LeftHand:
                return LeftHand;
            case ModelPointType.RightHand:
                return RightHand;
            default:
                return transform;
        }
    }
    /// <summary>
    /// 获得子弹发射点
    /// </summary>
    public Transform GetShootPoint()
    {
        return ShootPoint[0];
    }
}

/// <summary>
/// 模型位置点
/// </summary>
public enum ModelPointType
{
    None,
    Root = 1,
    Foot,
    Head,
    Body,
    LeftHand,
    RightHand,
}