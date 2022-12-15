/** 
*Copyright(C) 2020 by DefaultCompany 
*All rights reserved. 
*Author:       DESKTOP-AJS8G4U 
*UnityVersion：2022.1.0f1c1 
*创建时间:         2022-07-02 
*/
using System;

public class MinMaxRangeAttribute : Attribute
{
    public MinMaxRangeAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
    public float Min { get; private set; }
    public float Max { get; private set; }
}