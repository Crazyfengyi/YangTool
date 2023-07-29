/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-07-29 
*/  
using System;
using System.Collections;  
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
  
public class RotateImage : MonoBehaviour 
{

    public void Awake()
    {
        
    }

    public void Update()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0,0,-20*Time.deltaTime));
    }
} 