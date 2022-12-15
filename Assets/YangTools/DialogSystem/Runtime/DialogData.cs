/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-06-26 
*/
using System;
using UnityEngine;

/// <summary>
/// 节点数据
/// </summary>
[Serializable]
public class DialogNodeData
{
    public string Guid;
    public string DialogText;
    public Vector2 Positoin;
}
/// <summary>
/// 连接数据
/// </summary>
[Serializable]
public class NodeLinkData
{
    public string BaseNodeGuid;
    public string PortName;
    public string TargetNodeGuid;
}