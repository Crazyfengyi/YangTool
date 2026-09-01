using System;

/// <summary>
/// 任务系统强类型事件总线
/// </summary>
public static class QuestEventBus
{
    /// <summary>
    /// 怪物击杀事件
    /// </summary>
    public static event Action<string> OnMonsterKilled;

    /// <summary>
    /// 道具获得事件
    /// </summary>
    public static event Action<string, int> OnItemAdded;

    /// <summary>
    /// NPC对话事件
    /// </summary>
    public static event Action<string> OnNpcTalked;

    /// <summary>
    /// 地点进入事件
    /// </summary>
    public static event Action<string> OnLocationEntered;

    /// <summary>
    /// 通用任务进度事件
    /// </summary>
    public static event Action<QuestProgressEvent> OnProgressReported;

    /// <summary>
    /// 发布任务进度事件并分发对应的强类型事件
    /// </summary>
    /// <param name="progressEvent">任务进度事件</param>
    public static void Publish(QuestProgressEvent progressEvent)
    {
        if (progressEvent == null)
        {
            return;
        }

        OnProgressReported?.Invoke(progressEvent);
        switch (progressEvent.EventType)
        {
            case QuestProgressEventType.Kill:
                OnMonsterKilled?.Invoke(progressEvent.TargetId);
                break;
            case QuestProgressEventType.Collect:
            case QuestProgressEventType.ItemNum:
                OnItemAdded?.Invoke(progressEvent.TargetId, ToEventCount(progressEvent.Amount));
                break;
            case QuestProgressEventType.Talk:
                OnNpcTalked?.Invoke(progressEvent.TargetId);
                break;
            case QuestProgressEventType.ReachLocation:
                OnLocationEntered?.Invoke(progressEvent.TargetId);
                break;
        }
    }

    /// <summary>
    /// 发布怪物击杀事件
    /// </summary>
    /// <param name="monsterId">怪物ID</param>
    public static void PublishMonsterKilled(string monsterId)
    {
        Publish(new QuestProgressEvent(QuestProgressEventType.Kill, monsterId));
    }

    /// <summary>
    /// 发布道具获得事件
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="count">获得数量</param>
    public static void PublishItemAdded(string itemId, int count = 1)
    {
        Publish(new QuestProgressEvent(QuestProgressEventType.Collect, itemId, count));
    }

    /// <summary>
    /// 发布NPC对话事件
    /// </summary>
    /// <param name="npcId">NPC ID</param>
    public static void PublishNpcTalked(string npcId)
    {
        Publish(new QuestProgressEvent(QuestProgressEventType.Talk, npcId));
    }

    /// <summary>
    /// 发布地点进入事件
    /// </summary>
    /// <param name="locationId">地点ID</param>
    public static void PublishLocationEntered(string locationId)
    {
        Publish(new QuestProgressEvent(QuestProgressEventType.ReachLocation, locationId));
    }

    private static int ToEventCount(float amount)
    {
        return amount > 0f ? Math.Max(1, (int)Math.Round(amount)) : 1;
    }
}
