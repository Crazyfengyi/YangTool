/*
 *Copyright(C) 2020 by xybt
 *All rights reserved.
 *Author:PC-20260301BNFU
 *UnityVersion：2022.3.62f3c1
 *创建时间:2026-07-14
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 默认任务节点
/// 作为普通 UI 预制体实例化到任务列表容器
/// </summary>
public class TaskNode : MonoBehaviour
{
    private const int MaxRewardCount = 4;

    public QuestRuntime runtime;
    public TextMeshProUGUI titleText;
    public Button claimButton;
    public TextMeshProUGUI btnText;
    public TextMeshProUGUI progressText;
    public GameObject redNode;
    public Image bar;
    public Image ads;
    public GameObject isGetNode;

    /// <summary>
    /// 奖励物品视图引用
    /// </summary>
    public List<ItemShow> itemUIPropList;

    private float onlineDisplaySeconds; //在线时长显示值
    private QuestManager subscribedQuestManager; //已订阅的任务管理器

    /// <summary>
    /// 初始化任务节点并刷新展示
    /// </summary>
    /// <param name="questRuntime">任务运行时数据</param>
    public void Init(QuestRuntime questRuntime)
    {
        runtime = questRuntime;
        onlineDisplaySeconds = 0f;
        SubscribeOnlineTimeProgress();
        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(OnActionButtonClicked);
            claimButton.onClick.AddListener(OnActionButtonClicked);
        }

        RefreshDisplay();
    }

    /// <summary>订阅在线时长显示事件</summary>
    private void SubscribeOnlineTimeProgress()
    {
        UnsubscribeOnlineTimeProgress();
        subscribedQuestManager = QuestManager.Instance;
        if (subscribedQuestManager != null)
        {
            subscribedQuestManager.OnlineTimeProgressed += OnOnlineTimeProgressed;
        }
    }

    /// <summary>取消订阅在线时长显示事件</summary>
    private void UnsubscribeOnlineTimeProgress()
    {
        if (subscribedQuestManager == null)
        {
            return;
        }

        subscribedQuestManager.OnlineTimeProgressed -= OnOnlineTimeProgressed;
        subscribedQuestManager = null;
    }

    /// <summary>响应任务管理器的在线时长显示事件</summary>
    /// <param name="elapsedSeconds">本次累计的秒数</param>
    private void OnOnlineTimeProgressed(float elapsedSeconds)
    {
        if (runtime == null || runtime.Data == null || runtime.State != QuestState.Active || elapsedSeconds <= 0f)
        {
            return;
        }

        ConditionRuntime condition = GetDisplayCondition();
        if (!IsOnlineTimeCondition(condition) || condition.IsCompleted)
        {
            return;
        }

        onlineDisplaySeconds = Mathf.Min(condition.TargetCount,
            onlineDisplaySeconds + elapsedSeconds);
        RefreshDisplay(false);
    }

    /// <summary>销毁节点时移除原生按钮监听</summary>
    private void OnDestroy()
    {
        UnsubscribeOnlineTimeProgress();
        claimButton?.onClick.RemoveListener(OnActionButtonClicked);
    }

    /// <summary>刷新当前任务节点显示</summary>
    public void Refresh()
    {
        RefreshDisplay();
    }

    /// <summary>
    /// 刷新任务标题 进度 状态和奖励
    /// </summary>
    private void RefreshDisplay(bool syncOnlineTime = true)
    {
        if (runtime == null || runtime.Data == null)
        {
            return;
        }

        ConditionRuntime condition = GetDisplayCondition();
        if (syncOnlineTime && IsOnlineTimeCondition(condition))
        {
            onlineDisplaySeconds = condition.OnlineTimeSeconds;
        }

        string progress = GetProgressText(condition);
        string status = GetStatusText(condition);

        string title = runtime.Data.TaskType == TaskType.Collect && runtime.State == QuestState.Active &&
                       condition != null && condition.CurrentCount <= 0f
            ? "???"
            : runtime.Data.Title;

        switch (runtime.Data.TaskType)
        {
            case TaskType.EveryDay:
                int targetCount = condition != null ? Mathf.Max(0, condition.TargetCount) : 0;
                title = title == "???" ? title : FormatTaskTitle(title, "在线 {0} 秒", targetCount);
                break;
            case TaskType.Collect:
                if (title != "???")
                {
                    string targetId = condition?.Data?.TargetId;
                    title = FormatTaskTitle(title, "收集{0}", targetId);
                }

                break;
        }


        if (titleText != null)
        {
            titleText.text = $"{title}:{runtime.State}";
        }

        if (progressText != null)
        {
            progressText.text = progress;
        }

        if (btnText != null)
        {
            btnText.text = status;
        }

        redNode?.SetActive(runtime.State == QuestState.Completed);
        if (claimButton != null)
        {
            claimButton.gameObject.SetActive(runtime.State != QuestState.Rewarded);
            claimButton.interactable = CanClickActionButton(condition);
        }

        ads?.gameObject.SetActive(runtime.State == QuestState.Completed);

        float currentProgress = IsOnlineTimeCondition(condition)
            ? onlineDisplaySeconds
            : condition != null
                ? condition.CurrentCount
                : 0f;
        float nowProgress = condition != null && condition.TargetCount > 0
            ? Mathf.Clamp01(currentProgress / condition.TargetCount)
            : 1f;
        if (bar != null)
        {
            bar.fillAmount = nowProgress;
        }

        isGetNode?.SetActive(runtime.State == QuestState.Rewarded);
        RefreshRewardDisplay();
    }

    /// <summary>
    /// 获取当前任务节点用于显示的条件
    /// </summary>
    private ConditionRuntime GetDisplayCondition()
    {
        ObjectiveRuntime displayObjective = runtime.GetActiveObjective();
        if (displayObjective == null && runtime.Objectives.Count > 0)
        {
            displayObjective = runtime.Objectives[runtime.Objectives.Count - 1];
        }

        return displayObjective?.Conditions != null && displayObjective.Conditions.Count > 0
            ? displayObjective.Conditions[0]
            : null;
    }

    /// <summary>根据任务奖励刷新最多四个通用奖励视图</summary>
    private void RefreshRewardDisplay()
    {
        if (itemUIPropList == null)
        {
            return;
        }

        for (int i = 0; i < itemUIPropList.Count; i++)
        {
            itemUIPropList[i]?.Hide();
        }

        if (runtime?.Data?.Rewards == null)
        {
            return;
        }

        int rewardCount = Mathf.Min(MaxRewardCount, runtime.Data.Rewards.Count, itemUIPropList.Count);
        for (int i = 0; i < rewardCount; i++)
        {
            QuestRewardData reward = runtime.Data.Rewards[i];
            ItemShow rewardView = itemUIPropList[i];
            if (reward == null || rewardView == null)
            {
                continue;
            }

            rewardView.Show(reward);
        }
    }

    /// <summary>获取任务节点的进度文本</summary>
    private string GetProgressText(ConditionRuntime condition)
    {
        if (IsOnlineTimeCondition(condition))
        {
            return $"{FormatDuration(onlineDisplaySeconds)} / {FormatDuration(condition.TargetCount)}";
        }

        if (runtime.State == QuestState.Completed || runtime.State == QuestState.Rewarded || condition == null)
        {
            return "完成";
        }

        return $"<color=#2FB02F>{Mathf.FloorToInt(condition.CurrentCount)}</color>/{condition.TargetCount}";
    }

    /// <summary>
    /// 获取当前任务状态文案
    /// </summary>
    /// <param name="condition">当前显示条件</param>
    /// <returns>状态文案</returns>
    private string GetStatusText(ConditionRuntime condition)
    {
        return runtime.State.ToString();
        switch (runtime.State)
        {
            case QuestState.Locked:
                return "锁定";
            case QuestState.Available:
                return "接取任务";
            case QuestState.Completed:
                return "可领取";
            case QuestState.Rewarded:
                return "已领取";
            case QuestState.Active:
                return CanConfirmCompletion(condition) ? "确认完成" : "进行中";
            default:
                return "进行中";
        }
    }

    /// <summary>
    /// 判断当前按钮是否可以执行操作
    /// </summary>
    /// <param name="condition">当前显示条件</param>
    /// <returns>可以点击返回true</returns>
    private bool CanClickActionButton(ConditionRuntime condition)
    {
        return runtime.State == QuestState.Available
               || runtime.State == QuestState.Completed
               || CanConfirmCompletion(condition);
    }

    /// <summary>
    /// 判断是否需要手动确认已满足的任务目标
    /// </summary>
    /// <param name="condition">当前显示条件</param>
    /// <returns>需要手动确认返回true</returns>
    private bool CanConfirmCompletion(ConditionRuntime condition)
    {
        if (runtime.State != QuestState.Active)
        {
            return false;
        }

        ObjectiveRuntime objective = runtime.GetActiveObjective();
        return objective != null && objective.Data != null && !objective.Data.AutoComplete
               && objective.IsConditionsSatisfied;
    }

    /// <summary>判断是否为在线时长条件</summary>
    private static bool IsOnlineTimeCondition(ConditionRuntime condition)
    {
        return condition?.Data?.EventType == QuestProgressEventType.OnLineTime;
    }

    /// <summary>将秒数格式化为时分秒</summary>
    private static string FormatDuration(float durationSeconds)
    {
        long totalSeconds = Math.Max(0L, (long) Math.Floor(durationSeconds));
        long hours = totalSeconds / 3600L;
        long remainSeconds = totalSeconds % 3600L;
        long displayMinutes = remainSeconds / 60L;
        long displaySeconds = remainSeconds % 60L;
        return $"{hours:D2}:{displayMinutes:D2}:{displaySeconds:D2}";
    }

    /// <summary>保留已配置标题 无法解析本地化键时使用中文回退文本</summary>
    private static string FormatTaskTitle(string key, string fallback, params object[] args)
    {
        if (string.IsNullOrEmpty(key) || key.StartsWith("textKey", StringComparison.OrdinalIgnoreCase))
        {
            return string.Format(fallback, args);
        }

        if (key.Contains("{0}"))
        {
            try
            {
                return string.Format(key, args);
            }
            catch (FormatException)
            {
                return key;
            }
        }

        return key;
    }

    /// <summary>
    /// 响应任务操作按钮
    /// </summary>
    private void OnActionButtonClicked()
    {
        if (runtime == null || QuestManager.Instance == null)
        {
            return;
        }

        ConditionRuntime condition = GetDisplayCondition();
        switch (runtime.State)
        {
            case QuestState.Available:
                QuestManager.Instance.AcceptQuest(runtime.Id);
                break;
            case QuestState.Active:
                if (CanConfirmCompletion(condition))
                {
                    QuestManager.Instance.CompleteQuest(runtime.Id);
                }

                break;
            case QuestState.Completed:
                ClaimReward();
                break;
        }
    }

    /// <summary>
    /// 领取当前任务奖励
    /// </summary>
    private void ClaimReward()
    {
        if (runtime == null || runtime.State != QuestState.Completed)
        {
            Debug.LogWarning("当前任务不可领取奖励");
            return;
        }

        Debug.LogWarning("任务领取奖励");
        TryLookAd(success =>
        {
            if (success && QuestManager.Instance != null && QuestManager.Instance.ClaimReward(runtime.Id))
            {
                RefreshDisplay();
            }
        });
    }

    /// <summary>
    /// 调用可选平台广告服务
    /// </summary>
    private static void TryLookAd(Action<bool> callback)
    {
        callback?.Invoke(true);
    }
}
