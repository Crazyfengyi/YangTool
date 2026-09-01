using System;
using UnityEngine;

#region 基类

/// <summary>
/// 任务奖励运行时基类
/// </summary>
[Serializable]
public abstract class Reward
{
    /// <summary>
    /// 奖励显示文本
    /// </summary>
    public abstract string DisplayText { get; }

    /// <summary>
    /// 发放奖励
    /// </summary>
    /// <param name="rewardService">奖励服务</param>
    /// <returns>发放成功返回true</returns>
    public abstract bool Give(IQuestRewardService rewardService);
}

#endregion

#region 实现

/// <summary>
/// 现金奖励
/// </summary>
[Serializable]
public sealed class MoneyReward : Reward
{
    /// <summary>
    /// 发放的现金数量
    /// </summary>
    [InspectorName("现金数量")] [Min(1)] public int Money = 1;

    public override string DisplayText => $"现金 x{Money}";

    public override bool Give(IQuestRewardService rewardService)
    {
        return rewardService != null && Money > 0 && rewardService.TryGrantMoney(Money);
    }
}

/// <summary>
/// 金币奖励
/// </summary>
[Serializable]
public sealed class GoldReward : Reward
{
    /// <summary>
    /// 发放的金币数量
    /// </summary>
    [InspectorName("金币数量")] [Min(1)] public int Gold = 1;

    public override string DisplayText => $"金币 x{Gold}";

    public override bool Give(IQuestRewardService rewardService)
    {
        return rewardService != null && Gold > 0 && rewardService.TryGrantGold(Gold);
    }
}

/// <summary>
/// 经验奖励
/// </summary>
[Serializable]
public sealed class ExpReward : Reward
{
    /// <summary>
    /// 发放的经验数量
    /// </summary>
    [InspectorName("经验数量")] [Min(1)] public int Exp = 1;

    public override string DisplayText => $"经验 x{Exp}";

    /// <summary>
    /// 授予经验奖励的方法
    /// </summary>
    /// <param name="rewardService">奖励服务接口，用于处理具体的奖励发放逻辑</param>
    public override bool Give(IQuestRewardService rewardService)
    {
        // 检查rewardService是否有效且Exp大于0，然后尝试授予经验值
        return rewardService != null && Exp > 0 && rewardService.TryGrantExp(Exp);
    }
}

/// <summary>
/// 道具奖励
/// </summary>
[Serializable]
public sealed class ItemReward : Reward
{
    /// <summary>
    /// 发放的道具ID
    /// </summary>
    [InspectorName("道具 ID")] public string ItemId;

    /// <summary>
    /// 发放的道具数量
    /// </summary>
    [InspectorName("数量")] [Min(1)] public int Count = 1;

    public override string DisplayText => $"{ItemId} x{Count}";

    public override bool Give(IQuestRewardService rewardService)
    {
        return rewardService != null && !string.IsNullOrWhiteSpace(ItemId)
                                     && Count > 0 && rewardService.TryGrantItem(ItemId, Count);
    }
}

/// <summary>
/// 自定义奖励
/// </summary>
[Serializable]
public sealed class CustomReward : Reward
{
    /// <summary>
    /// 自定义奖励键
    /// </summary>
    [InspectorName("奖励键")] public string RewardKey;

    /// <summary>
    /// 自定义奖励数量
    /// </summary>
    [InspectorName("数量")] [Min(1)] public int Count = 1;

    public override string DisplayText => $"{RewardKey} x{Count}";

    public override bool Give(IQuestRewardService rewardService)
    {
        return rewardService != null && !string.IsNullOrWhiteSpace(RewardKey)
                                     && Count > 0 && rewardService.TryGrantCustom(RewardKey, Count);
    }
}

#endregion