/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2023.2.0b16 
 *创建时间:         2024-06-10 
*/  
using System;
  
/// <summary>
/// 次数或时间结束检测
/// </summary>
[Serializable]
public class CountOrTimeEndChecker : BuffEndChecker 
{
    /// <summary>
    /// 结束时间
    /// </summary>
    public RefreshValue timeValue;
    public CountOrTimeEndChecker(BuffConfig buffConfig) : base(buffConfig)
    {
        timeValue = new RefreshValue(buffConfig.time, true);
    }
    
    public override void Update(float tickTime)
    {
        base.Update(tickTime);
        
        if(!IsEnd())
        {
            timeValue.CutDown(tickTime);
        }
    }
    public override bool IsActive()
    {
        return false;
    }
    public override bool IsEnd()
    {
        return timeValue.currentValue <= 0;
    }
    public override void Refresh()
    {
        base.Refresh();
        timeValue.RefreshToMax();
    }
} 