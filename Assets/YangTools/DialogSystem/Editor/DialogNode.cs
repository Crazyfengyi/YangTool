/** 
*Copyright(C) 2020 by DefaultCompany 
*All rights reserved. 
*Author:       DESKTOP-AJS8G4U 
*UnityVersion：2022.1.0f1c1 
*创建时间:         2022-06-26 
*/
using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;
/// <summary>
/// 对话节点
/// </summary>
public class DialogNode : Node
{
    public string GUID;//GUID
    public string DialogText;//文本
    public bool EntryPoint = false;//入口点
}