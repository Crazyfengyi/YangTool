/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-06-26 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
/// <summary>
/// 对话框容器
/// </summary>
public class DialogContainer : ScriptableObject
{
    public List<DialogNodeData> DialogNodeData = new List<DialogNodeData>();
    public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();



}