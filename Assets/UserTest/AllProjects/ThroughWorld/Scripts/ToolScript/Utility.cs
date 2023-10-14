/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-09-29 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;

public static class Utility
{
    public static Matrix4x4 _Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));

    public static Vector3 ToChange(this Vector3 input, float angle)
    {
        _Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, angle, 0));
        return _Matrix.MultiplyPoint3x4(input);
    }

    public static float AniCrossTime => 0.2f;
}