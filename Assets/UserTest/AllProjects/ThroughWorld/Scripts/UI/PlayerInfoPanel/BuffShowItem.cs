/* 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2023.2.0b16 
 *创建时间:         2024-06-10 
*/  
using System;
using System.Collections;  
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using YangTools;

public class BuffShowItem : MonoBehaviour
{
    public Image icon;

    public void SetData(BuffBase buff)
    {
        icon.sprite = buff.icon;
    }
} 