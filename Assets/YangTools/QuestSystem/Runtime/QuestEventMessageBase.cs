using System;

/// <summary>
/// 任务事件基类
/// </summary>
/// <remarks>
/// 核心层不依赖项目事件系统 具体适配器可将事件转发到项目总线
/// </remarks>
public class QuestEventMessageBase
{
    /// <summary>
    /// 可选项目事件发送器
    /// </summary>
    public static Action<QuestEventMessageBase> Sender;

    /// <summary>
    /// 发送事件的保底实现
    /// </summary>
    public virtual void SendEvent()
    {
        Sender?.Invoke(this);
    }
}
