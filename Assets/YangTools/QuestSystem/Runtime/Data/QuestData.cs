using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务配置数据
/// </summary>
[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest/QuestData")]
public class QuestData : ScriptableObject
{
    /// <summary>
    /// 任务稳定唯一标识
    /// </summary>
    [InspectorName("任务稳定ID")]
    public string Id;

    /// <summary>
    /// 任务业务类型
    /// </summary>
    [InspectorName("任务类型")]
    public TaskType TaskType;

    /// <summary>
    /// 前置条件满足后是否自动激活
    /// </summary>
    [InspectorName("默认激活任务")]
    [Tooltip("任务注册且前置任务满足后是否直接进入进行中状态")]
    public bool DefaultActive;

    /// <summary>
    /// 任务标题
    /// </summary>
    [InspectorName("任务标题")]
    [TextArea(1, 2)]
    public string Title;

    /// <summary>
    /// 任务描述
    /// </summary>
    [InspectorName("任务描述")]
    [TextArea(1, 2)]
    public string Description;

    /// <summary>
    /// 接取任务前必须完成的任务ID列表
    /// </summary>
    [InspectorName("任务前置条件ID列表")]
    public List<string> PrerequisiteQuestIds = new List<string>();

    /// <summary>
    /// 按顺序执行的任务目标配置列表
    /// </summary>
    [InspectorName("任务目标数据列表")]
    public List<QuestObjectiveData> Objectives = new List<QuestObjectiveData>();

    /// <summary>
    /// 任务完成后发放的奖励配置列表
    /// </summary>
    [InspectorName("任务奖励数据列表")]
    [SerializeReference]
    public List<Reward> Rewards = new List<Reward>();
}

/// <summary>
/// 任务目标配置数据
/// </summary>
[Serializable]
public class QuestObjectiveData
{
    /// <summary>
    /// 目标备注
    /// </summary>
    [InspectorName("备注")]
    [TextArea(1, 1)]
    public string remark;

    /// <summary>
    /// 目标标题
    /// </summary>
    [InspectorName("目标标题")]
    [TextArea(1, 1)]
    public string Title;

    /// <summary>
    /// 目标描述
    /// </summary>
    [InspectorName("目标描述")]
    [TextArea(1, 1)]
    public string Description;

    /// <summary>
    /// 条件满足后是否自动完成目标
    /// </summary>
    [InspectorName("条件满足后自动完成")]
    public bool AutoComplete = true;

    /// <summary>
    /// 目标使用的根条件 可通过组合条件嵌套多个子条件
    /// </summary>
    [InspectorName("条件")]
    [SerializeReference]
    public Condition Condition;
}

/// <summary>
/// 任务业务类型
/// </summary>
public enum TaskType
{
    [InspectorName("普通")]
    None,
    [InspectorName("每日")]
    EveryDay,
    [InspectorName("现金")]
    Money,
    [InspectorName("收集")]
    Collect
}
