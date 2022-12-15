/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-14 
*/
using System;

[Serializable]
/// <summary>
/// 时间结束检测
/// </summary>
public class TimeEndChecker : BuffEndChecker
{
    /// <summary>
    /// 结束时间
    /// </summary>
    public RefreshValue timeValue;
    public TimeEndChecker(BuffConfig buffConfig) : base(buffConfig)
    {
        timeValue = new RefreshValue(buffConfig.time, true);
    }
    public override void Update(float tickTime)
    {
        base.Update(tickTime);
        if (timeValue.curValue > -100)
        {
            timeValue.Update(tickTime);
        }
    }
    public override bool IsActive()
    {
        return false;
    }
    public override bool IsEnd()
    {
        if (timeValue.curValue <= -100) return false;
        return timeValue.curValue <= 0;
    }
    public override void Refresh()
    {
        base.Refresh();
        timeValue.Refresh();
    }
}