#if YANGTOOLS_QUEST_INTEGRATION
using System;
using GameMain;

/// <summary>
/// 项目背包任务适配器
/// </summary>
public sealed class BagQuestItemService : IQuestItemService
{
    /// <summary>
    /// 获取项目背包中的道具数量
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <returns>当前道具数量</returns>
    public float GetItemCount(int itemId)
    {
        BagMgr bagManager = BagMgr.Instance;
        return bagManager != null ? Math.Max(0f, bagManager.GetBagPropCount(itemId)) : 0f;
    }

    /// <summary>
    /// 检查项目背包中的道具是否足够
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="count">需要数量</param>
    /// <returns>道具足够返回true</returns>
    public bool HasItem(int itemId, float count)
    {
        BagMgr bagManager = BagMgr.Instance;
        return bagManager != null && itemId > 0 && count > 0f
               && bagManager.BagPropEnough(itemId, count, false);
    }

    /// <summary>
    /// 尝试从项目背包中消耗道具
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="count">消耗数量</param>
    /// <returns>消耗成功返回true</returns>
    public bool TryConsume(int itemId, float count)
    {
        if (!HasItem(itemId, count))
        {
            return false;
        }

        BagMgr.Instance.RemoveBagProp(itemId, count);
        return true;
    }
}
#endif
