/*
 *Copyright(C) 2020 by xybt
 *All rights reserved.
 *Author:PC-20260301BNFU
 *UnityVersion：2022.3.62f3c1
 *创建时间:2026-07-14
 */

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 默认任务窗口
/// 直接将当前分类的全部任务实例化到滚动列表中
/// </summary>
public class TaskWindow : MonoBehaviour
{
    public Button closeBtn;
    public Toggle everyDayToggle;
    public Toggle taskToggle;

    /// <summary>滚动区域</summary>
    public ScrollRect scrollRect;

    /// <summary>任务节点容器</summary>
    public RectTransform taskContent;

    /// <summary>任务节点预制体</summary>
    public TaskNode taskNode;

    public GameObject red1;
    public GameObject red2;

    private readonly List<QuestRuntime> everyDayTaskList = new List<QuestRuntime>();
    private readonly List<QuestRuntime> collectTaskList = new List<QuestRuntime>();
    private readonly List<TaskNode> taskItems = new List<TaskNode>();
    private int selectIndex;

    private QuestManager questManager;

    // 防止切换分类时重复触发 Toggle 回调
    private bool isUpdatingToggle;

    // 当前窗口是否已完成初始化
    private bool isOpen;

    /// <summary>
    /// Unity 启用组件时初始化窗口
    /// </summary>
    private async void OnEnable()
    {
        await new WaitUntil(() => QuestManager.Instance != null);
        Open();
    }

    /// <summary>
    /// Unity 禁用组件时清理窗口
    /// </summary>
    private void OnDisable()
    {
        CleanupWindow();
    }

    /// <summary>
    /// 打开任务窗口并订阅任务事件
    /// </summary>
    public void Open()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            return;
        }

        if (isOpen)
        {
            return;
        }

        isOpen = true;
        selectIndex = 1;

        everyDayToggle?.onValueChanged.RemoveListener(OnEveryDayToggleChanged);
        everyDayToggle?.onValueChanged.AddListener(OnEveryDayToggleChanged);
        taskToggle?.onValueChanged.RemoveListener(OnTaskToggleChanged);
        taskToggle?.onValueChanged.AddListener(OnTaskToggleChanged);
        SetSelectedCategory(1, false);

        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveListener(OnCloseBtnClick);
            closeBtn.onClick.AddListener(OnCloseBtnClick);
        }

        questManager = QuestManager.Instance;
        if (questManager != null)
        {
            questManager.DataLoaded += OnQuestDataLoaded;
            questManager.QuestChanged += OnQuestChanged;
            questManager.ObjectiveChanged += OnObjectiveChanged;
            questManager.QuestReset += OnQuestReset;
        }

        Reload(true);
    }

    /// <summary>
    /// 关闭任务窗口
    /// </summary>
    public void Close()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            return;
        }

        CleanupWindow();
    }

    /// <summary>
    /// 清理事件和任务节点
    /// </summary>
    private void CleanupWindow()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        closeBtn?.onClick.RemoveListener(OnCloseBtnClick);
        everyDayToggle?.onValueChanged.RemoveListener(OnEveryDayToggleChanged);
        taskToggle?.onValueChanged.RemoveListener(OnTaskToggleChanged);
        if (questManager != null)
        {
            questManager.DataLoaded -= OnQuestDataLoaded;
            questManager.QuestChanged -= OnQuestChanged;
            questManager.ObjectiveChanged -= OnObjectiveChanged;
            questManager.QuestReset -= OnQuestReset;
            questManager = null;
        }

        ClearTaskItems();
    }

    /// <summary>任务数据或进度变化时刷新列表</summary>
    private void OnQuestDataLoaded(QuestDataLoadedEvent eventData)
    {
        Reload();
    }

    private void OnQuestChanged(QuestChangedEvent eventData)
    {
        Reload();
    }

    private void OnObjectiveChanged(QuestObjectiveChangedEvent eventData)
    {
        if (eventData == null)
        {
            return;
        }

        for (int i = 0; i < taskItems.Count; i++)
        {
            TaskNode taskItem = taskItems[i];
            if (taskItem != null && taskItem.runtime != null && taskItem.runtime.Id == eventData.QuestId)
            {
                taskItem.Refresh();
                return;
            }
        }
    }

    private void OnQuestReset(QuestResetEvent eventData)
    {
        Reload();
    }

    private void OnCloseBtnClick()
    {
        Close();
    }

    /// <summary>选择每日任务分类</summary>
    private void OnEveryDayToggleChanged(bool isOn)
    {
        if (isUpdatingToggle || !isOn)
        {
            return;
        }

        SetSelectedCategory(1, true);
    }

    /// <summary>选择收集任务分类</summary>
    private void OnTaskToggleChanged(bool isOn)
    {
        if (isUpdatingToggle || !isOn)
        {
            return;
        }

        SetSelectedCategory(2, true);
    }

    /// <summary>切换任务分类并同步原生 Toggle 的显示状态</summary>
    private void SetSelectedCategory(int category, bool reload)
    {
        selectIndex = category;
        isUpdatingToggle = true;
        if (everyDayToggle != null)
        {
            everyDayToggle.SetIsOnWithoutNotify(category == 1);
            RefreshToggleVisual(everyDayToggle, category == 1);
        }

        if (taskToggle != null)
        {
            taskToggle.SetIsOnWithoutNotify(category == 2);
            RefreshToggleVisual(taskToggle, category == 2);
        }

        isUpdatingToggle = false;
        if (reload)
        {
            Reload(true);
        }
    }

    /// <summary>兼容原预制体的选中和未选中节点</summary>
    private static void RefreshToggleVisual(Toggle toggle, bool isOn)
    {
        if (toggle == null)
        {
            return;
        }

        Transform selected = toggle.transform.Find("SelectShow");
        Transform unselected = toggle.transform.Find("NoSelect");
        selected?.gameObject.SetActive(isOn);
        unselected?.gameObject.SetActive(!isOn);
    }

    /// <summary>
    /// 重建任务分类数据并直接生成所有任务节点
    /// </summary>
    /// <param name="newStartPos">是否将滚动位置重置到顶部</param>
    public void Reload(bool newStartPos = false)
    {
        everyDayTaskList.Clear();
        collectTaskList.Clear();

        QuestManager manager = QuestManager.Instance;
        if (manager != null)
        {
            List<QuestRuntime> quests = manager.GetAllQuests();
            if (quests != null)
            {
                quests.Sort(CompareTaskOrder);
                for (int i = 0; i < quests.Count; i++)
                {
                    QuestRuntime quest = quests[i];
                    if (quest?.Data == null)
                    {
                        continue;
                    }

                    if (quest.Data.TaskType == TaskType.EveryDay)
                    {
                        everyDayTaskList.Add(quest);
                    }
                    else if (quest.Data.TaskType == TaskType.Collect)
                    {
                        collectTaskList.Add(quest);
                    }
                }
            }
        }

        everyDayTaskList.Sort(CompareTaskOrder);
        collectTaskList.Sort(CompareTaskOrder);
        RefreshRedPoint();
        GenerateTaskItems(GetSelectedTaskList(), newStartPos);
    }

    /// <summary>解析滚动列表引用并绑定 Content</summary>
    private void ResolveScrollReferences()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        if (taskContent == null && scrollRect != null)
        {
            taskContent = scrollRect.content;
        }

        if (taskContent == null)
        {
            Transform contentTransform = transform.Find("WindowRoot/Scroll View/Content");
            taskContent = contentTransform as RectTransform;
        }

        if (scrollRect != null && taskContent != null)
        {
            scrollRect.content = taskContent;
        }
    }

    /// <summary>清理上一次生成的任务节点</summary>
    private void ClearTaskItems()
    {
        for (int i = 0; i < taskItems.Count; i++)
        {
            if (taskItems[i] != null)
            {
                Destroy(taskItems[i].gameObject);
            }
        }

        taskItems.Clear();
    }

    /// <summary>实例化指定列表中的全部任务节点</summary>
    private void GenerateTaskItems(List<QuestRuntime> taskList, bool newStartPos)
    {
        ClearTaskItems();
        ResolveScrollReferences();
        if (taskContent == null || taskNode == null)
        {
            Debug.LogWarning("TaskWindow 缺少 Content 或 TaskNode 预制体引用");
            return;
        }

        for (int i = 0; i < taskList.Count; i++)
        {
            QuestRuntime runtime = taskList[i];
            if (runtime == null)
            {
                continue;
            }

            TaskNode item = Instantiate(taskNode, taskContent, false);
            item.gameObject.SetActive(true);
            item.Init(runtime);
            taskItems.Add(item);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(taskContent);
        if (newStartPos && scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private List<QuestRuntime> GetSelectedTaskList()
    {
        return selectIndex == 2 ? collectTaskList : everyDayTaskList;
    }

    /// <summary>刷新每日任务和收集任务的未领取奖励红点</summary>
    private void RefreshRedPoint()
    {
        SetRedPointVisible(red1, HasCompletedTask(everyDayTaskList));
        SetRedPointVisible(red2, HasCompletedTask(collectTaskList));
    }

    /// <summary>判断任务列表中是否存在已完成但未领取奖励的任务</summary>
    private static bool HasCompletedTask(List<QuestRuntime> taskList)
    {
        if (taskList == null)
        {
            return false;
        }

        for (int i = 0; i < taskList.Count; i++)
        {
            if (taskList[i] != null && taskList[i].State == QuestState.Completed)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>设置红点节点显隐状态</summary>
    private static void SetRedPointVisible(GameObject redPoint, bool visible)
    {
        redPoint?.SetActive(visible);
    }

    /// <summary>按任务状态和当前进度排序</summary>
    private static int CompareTaskOrder(QuestRuntime left, QuestRuntime right)
    {
        int leftPriority = GetTaskPriority(left);
        int rightPriority = GetTaskPriority(right);
        int priorityComparison = leftPriority.CompareTo(rightPriority);
        if (priorityComparison != 0)
        {
            return priorityComparison;
        }

        if (leftPriority == 1)
        {
            int progressComparison = GetProgressRatio(right).CompareTo(GetProgressRatio(left));
            if (progressComparison != 0)
            {
                return progressComparison;
            }
        }

        return string.CompareOrdinal(left?.Id, right?.Id);
    }

    /// <summary>获取任务排序优先级</summary>
    private static int GetTaskPriority(QuestRuntime runtime)
    {
        if (runtime == null)
        {
            return 4;
        }

        if (runtime.State == QuestState.Completed)
        {
            return 0;
        }

        if (runtime.State == QuestState.Rewarded)
        {
            return 3;
        }

        if (runtime.State == QuestState.Active)
        {
            return GetProgressRatio(runtime) > 0f ? 1 : 2;
        }

        return 4;
    }

    /// <summary>获取任务当前目标第一个条件的进度比例</summary>
    private static float GetProgressRatio(QuestRuntime runtime)
    {
        ObjectiveRuntime objective = runtime?.GetActiveObjective();
        ConditionRuntime condition = objective?.Conditions != null && objective.Conditions.Count > 0
            ? objective.Conditions[0]
            : null;
        if (condition == null || condition.TargetCount <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01(condition.CurrentCount / condition.TargetCount);
    }
}
