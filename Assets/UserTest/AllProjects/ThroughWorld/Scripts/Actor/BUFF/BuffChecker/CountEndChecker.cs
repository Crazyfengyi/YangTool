/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       WIN-VJ19D9AB7HB
 *UnityVersion：2023.2.0b16
 *创建时间:         2024-06-10
*/

using System;

/// <summary>
/// 次数结束检测
/// </summary>
[Serializable]
public class CountEndChecker : BuffEndChecker
{
    public RefreshValue count;
    public CountEndChecker(BuffConfig buffConfig) : base(buffConfig)
    {
        count = new RefreshValue(1,true);
    }
  
    public override bool IsActive()
    {
        return false;
    }
    public override bool IsEnd()
    {
        return count.currentValue <= 0;
    }
    public override void Refresh()
    {
        base.Refresh();
        count.RefreshToMax();
    }
}