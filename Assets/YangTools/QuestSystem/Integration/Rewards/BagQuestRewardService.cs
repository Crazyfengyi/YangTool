#if YANGTOOLS_QUEST_INTEGRATION
using GameMain;

/// <summary>
/// 项目任务奖励服务
/// </summary>
public sealed class BagQuestRewardService : IQuestRewardService
{
    /// <summary>
    /// 发放现金奖励
    /// </summary>
    public bool TryGrantMoney(int amount) => false;

    /// <summary>
    /// 发放金币奖励
    /// </summary>
    public bool TryGrantGold(int amount) => false;

    /// <summary>
    /// 发放经验奖励
    /// </summary>
    public bool TryGrantExp(int amount) => false;

    /// <summary>
    /// 发放道具奖励
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="count">奖励数量</param>
    /// <returns>发放成功返回true</returns>
    public bool TryGrantItem(string itemId, int count)
    {
        if (!int.TryParse(itemId, out int propId) || propId <= 0 || count <= 0 || BagMgr.Instance == null)
        {
            return false;
        }

        BagMgr.Instance.AddBagProp(propId, count, true, "任务奖励");
        return true;
    }

    /// <summary>
    /// 发放自定义奖励
    /// </summary>
    /// <param name="rewardKey">奖励键</param>
    /// <param name="count">奖励数量</param>
    /// <returns>发放成功返回true</returns>
    public bool TryGrantCustom(string rewardKey, int count) => false;
}
#endif
