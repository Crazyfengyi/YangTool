#if YANGTOOLS_QUEST_INTEGRATION
using System;
using GameMain;
using YangTools;

/// <summary>
/// 项目全局事件系统任务桥接器
/// </summary>
public sealed class YangQuestEventBridge : IDisposable
{
    private readonly QuestManager manager;
    private readonly YangEventGroup eventGroup = new YangEventGroup();
    private bool disposed;

    /// <summary>
    /// 创建任务事件桥接器并注册项目事件监听
    /// </summary>
    /// <param name="manager">任务管理器</param>
    public YangQuestEventBridge(QuestManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
        QuestEventMessageBase.Sender = SendQuestEvent;
        eventGroup.AddListener<EventMessageBase>(OnProgressEvent);
        manager.QuestChanged += Publish;
        manager.ObjectiveChanged += Publish;
        manager.RewardIssued += Publish;
        manager.QuestReset += Publish;
        manager.DataLoaded += Publish;
    }

    /// <summary>
    /// 释放项目事件监听
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        eventGroup.RemoveAllListener();
        manager.QuestChanged -= Publish;
        manager.ObjectiveChanged -= Publish;
        manager.RewardIssued -= Publish;
        manager.QuestReset -= Publish;
        manager.DataLoaded -= Publish;
        if (QuestEventMessageBase.Sender == SendQuestEvent)
        {
            QuestEventMessageBase.Sender = null;
        }
    }

    /// <summary>
    /// 将项目进度事件转发到任务管理器
    /// </summary>
    /// <param name="eventData">项目事件数据</param>
    private void OnProgressEvent(EventData eventData)
    {
        manager.ReportProgress(eventData?.Args as QuestProgressEvent);
    }

    /// <summary>
    /// 将任务事件发送到项目事件系统
    /// </summary>
    /// <param name="eventData">任务事件</param>
    private static void SendQuestEvent(QuestEventMessageBase eventData)
    {
        if (eventData != null)
        {
            YangExtend.SendEvent(eventData.GetType(), eventData);
        }
    }

    private static void Publish(QuestChangedEvent eventData) => YangExtend.SendEvent(typeof(QuestChangedEvent), eventData);
    private static void Publish(QuestObjectiveChangedEvent eventData) => YangExtend.SendEvent(typeof(QuestObjectiveChangedEvent), eventData);
    private static void Publish(QuestRewardEvent eventData) => YangExtend.SendEvent(typeof(QuestRewardEvent), eventData);
    private static void Publish(QuestResetEvent eventData) => YangExtend.SendEvent(typeof(QuestResetEvent), eventData);
    private static void Publish(QuestDataLoadedEvent eventData) => YangExtend.SendEvent(typeof(QuestDataLoadedEvent), eventData);
}
#endif
