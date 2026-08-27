#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 任务配置的中文检视器
/// </summary>
[CustomEditor(typeof(QuestData))]
public sealed class QuestDataEditor : Editor
{
    private const float VerticalSpacing = 2f;

    #region 序列化属性

    private SerializedProperty idProperty; //任务ID属性
    private SerializedProperty taskTypeProperty; //任务类型属性
    private SerializedProperty titleProperty; //任务标题属性
    private SerializedProperty descriptionProperty; //任务描述属性
    private SerializedProperty prerequisiteQuestIdsProperty; //前置任务属性
    private SerializedProperty objectivesProperty; //任务目标属性
    private SerializedProperty rewardsProperty; //任务奖励属性
    private ReorderableList prerequisiteQuestIdsList; //前置任务列表
    private ReorderableList objectivesList; //任务目标列表
    private ReorderableList rewardsList; //任务奖励列表

    #endregion

    #region Unity回调

    /// <summary>
    /// 初始化序列化属性和列表
    /// </summary>
    private void OnEnable()
    {
        idProperty = serializedObject.FindProperty("Id");
        taskTypeProperty = serializedObject.FindProperty("TaskType");
        titleProperty = serializedObject.FindProperty("Title");
        descriptionProperty = serializedObject.FindProperty("Description");
        prerequisiteQuestIdsProperty = serializedObject.FindProperty("PrerequisiteQuestIds");
        objectivesProperty = serializedObject.FindProperty("Objectives");
        rewardsProperty = serializedObject.FindProperty("Rewards");

        prerequisiteQuestIdsList = CreatePrerequisiteQuestIdsList();
        objectivesList = CreateObjectivesList();
        rewardsList = CreateRewardsList();
    }

    /// <summary>
    /// 绘制中文任务配置界面
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(idProperty, new GUIContent("任务ID"));
        EditorGUILayout.PropertyField(taskTypeProperty, new GUIContent("任务类型"));
        EditorGUILayout.PropertyField(titleProperty, new GUIContent("任务标题"));
        EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("任务描述"));
        EditorGUILayout.Space();

        prerequisiteQuestIdsList.DoLayoutList();
        objectivesList.DoLayoutList();
        rewardsList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region 顶层列表

    /// <summary>
    /// 创建前置任务列表
    /// </summary>
    private ReorderableList CreatePrerequisiteQuestIdsList()
    {
        ReorderableList list = new ReorderableList(serializedObject, prerequisiteQuestIdsProperty, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "前置任务ID列表");
        list.drawElementCallback = (rect, index, _, _) =>
        {
            SerializedProperty element = prerequisiteQuestIdsProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, new GUIContent($"前置任务ID {index + 1}"));
        };
        return list;
    }

    /// <summary>
    /// 创建任务目标列表
    /// </summary>
    private ReorderableList CreateObjectivesList()
    {
        ReorderableList list = new ReorderableList(serializedObject, objectivesProperty, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "任务目标列表");
        list.drawElementCallback = (rect, index, _, _) => DrawObjective(rect, objectivesProperty.GetArrayElementAtIndex(index), index);
        list.elementHeightCallback = index => GetObjectiveHeight(objectivesProperty.GetArrayElementAtIndex(index));
        list.onAddCallback = AddExpandedElement;
        return list;
    }

    /// <summary>
    /// 创建任务奖励列表
    /// </summary>
    private ReorderableList CreateRewardsList()
    {
        ReorderableList list = new ReorderableList(serializedObject, rewardsProperty, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "任务奖励列表");
        list.drawElementCallback = (rect, index, _, _) => DrawReward(rect, rewardsProperty.GetArrayElementAtIndex(index), index);
        list.elementHeightCallback = index => GetRewardHeight(rewardsProperty.GetArrayElementAtIndex(index));
        list.onAddCallback = AddExpandedElement;
        return list;
    }

    /// <summary>
    /// 添加并展开新的列表项
    /// </summary>
    private static void AddExpandedElement(ReorderableList list)
    {
        ReorderableList.defaultBehaviours.DoAddButton(list);
        if (list.index >= 0 && list.index < list.serializedProperty.arraySize)
        {
            list.serializedProperty.GetArrayElementAtIndex(list.index).isExpanded = true;
        }
    }

    #endregion

    #region 任务目标与条件

    /// <summary>
    /// 绘制单个任务目标
    /// </summary>
    private void DrawObjective(Rect rect, SerializedProperty objectiveProperty, int index)
    {
        Rect contentRect = rect;
        objectiveProperty.isExpanded = EditorGUI.Foldout(GetNextLine(ref contentRect), objectiveProperty.isExpanded, $"任务目标 {index + 1}", true);
        if (!objectiveProperty.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        DrawProperty(ref contentRect, objectiveProperty.FindPropertyRelative("remark"), "备注");
        DrawProperty(ref contentRect, objectiveProperty.FindPropertyRelative("Title"), "目标标题");
        DrawProperty(ref contentRect, objectiveProperty.FindPropertyRelative("Description"), "目标描述");
        DrawProperty(ref contentRect, objectiveProperty.FindPropertyRelative("AutoComplete"), "条件满足后自动完成");
        DrawProperty(ref contentRect, objectiveProperty.FindPropertyRelative("ConditionGroupType"), "条件组合方式");

        ReorderableList conditionsList = CreateConditionsList(objectiveProperty.FindPropertyRelative("Conditions"));
        float conditionsHeight = conditionsList.GetHeight();
        conditionsList.DoList(new Rect(contentRect.x, contentRect.y, contentRect.width, conditionsHeight));
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 获取任务目标的显示高度
    /// </summary>
    private float GetObjectiveHeight(SerializedProperty objectiveProperty)
    {
        float height = EditorGUIUtility.singleLineHeight + VerticalSpacing;
        if (!objectiveProperty.isExpanded)
        {
            return height;
        }

        height += GetPropertyHeight(objectiveProperty.FindPropertyRelative("remark"));
        height += GetPropertyHeight(objectiveProperty.FindPropertyRelative("Title"));
        height += GetPropertyHeight(objectiveProperty.FindPropertyRelative("Description"));
        height += GetPropertyHeight(objectiveProperty.FindPropertyRelative("AutoComplete"));
        height += GetPropertyHeight(objectiveProperty.FindPropertyRelative("ConditionGroupType"));
        height += CreateConditionsList(objectiveProperty.FindPropertyRelative("Conditions")).GetHeight();
        return height;
    }

    /// <summary>
    /// 创建任务条件列表
    /// </summary>
    private ReorderableList CreateConditionsList(SerializedProperty conditionsProperty)
    {
        ReorderableList list = new ReorderableList(serializedObject, conditionsProperty, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "条件列表");
        list.drawElementCallback = (rect, index, _, _) => DrawCondition(rect, conditionsProperty.GetArrayElementAtIndex(index), index);
        list.elementHeightCallback = index => GetConditionHeight(conditionsProperty.GetArrayElementAtIndex(index));
        list.onAddCallback = AddExpandedElement;
        return list;
    }

    /// <summary>
    /// 绘制单个任务条件
    /// </summary>
    private static void DrawCondition(Rect rect, SerializedProperty conditionProperty, int index)
    {
        Rect contentRect = rect;
        conditionProperty.isExpanded = EditorGUI.Foldout(GetNextLine(ref contentRect), conditionProperty.isExpanded, $"条件 {index + 1}", true);
        if (!conditionProperty.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        DrawProperty(ref contentRect, conditionProperty.FindPropertyRelative("ConditionType"), "条件类型");
        DrawProperty(ref contentRect, conditionProperty.FindPropertyRelative("EventType"), "进度事件类型");
        DrawProperty(ref contentRect, conditionProperty.FindPropertyRelative("TargetId"), "事件目标ID");
        DrawProperty(ref contentRect, conditionProperty.FindPropertyRelative("TargetCount"), "目标数量");
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 获取任务条件的显示高度
    /// </summary>
    private static float GetConditionHeight(SerializedProperty conditionProperty)
    {
        float height = EditorGUIUtility.singleLineHeight + VerticalSpacing;
        if (!conditionProperty.isExpanded)
        {
            return height;
        }

        height += GetPropertyHeight(conditionProperty.FindPropertyRelative("ConditionType"));
        height += GetPropertyHeight(conditionProperty.FindPropertyRelative("EventType"));
        height += GetPropertyHeight(conditionProperty.FindPropertyRelative("TargetId"));
        height += GetPropertyHeight(conditionProperty.FindPropertyRelative("TargetCount"));
        return height;
    }

    #endregion

    #region 任务奖励与绘制工具

    /// <summary>
    /// 绘制单个任务奖励
    /// </summary>
    private static void DrawReward(Rect rect, SerializedProperty rewardProperty, int index)
    {
        Rect contentRect = rect;
        rewardProperty.isExpanded = EditorGUI.Foldout(GetNextLine(ref contentRect), rewardProperty.isExpanded, $"奖励 {index + 1}", true);
        if (!rewardProperty.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        DrawProperty(ref contentRect, rewardProperty.FindPropertyRelative("RewardType"), "奖励类型");
        DrawProperty(ref contentRect, rewardProperty.FindPropertyRelative("TargetKey"), "奖励目标ID");
        DrawProperty(ref contentRect, rewardProperty.FindPropertyRelative("Count"), "奖励数量");
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 获取任务奖励的显示高度
    /// </summary>
    private static float GetRewardHeight(SerializedProperty rewardProperty)
    {
        float height = EditorGUIUtility.singleLineHeight + VerticalSpacing;
        if (!rewardProperty.isExpanded)
        {
            return height;
        }

        height += GetPropertyHeight(rewardProperty.FindPropertyRelative("RewardType"));
        height += GetPropertyHeight(rewardProperty.FindPropertyRelative("TargetKey"));
        height += GetPropertyHeight(rewardProperty.FindPropertyRelative("Count"));
        return height;
    }

    /// <summary>
    /// 绘制带中文标签的序列化属性
    /// </summary>
    private static void DrawProperty(ref Rect rect, SerializedProperty property, string label)
    {
        float height = EditorGUI.GetPropertyHeight(property, true);
        Rect propertyRect = new Rect(rect.x, rect.y, rect.width, height);
        EditorGUI.PropertyField(propertyRect, property, new GUIContent(label), true);
        rect.y += height + VerticalSpacing;
    }

    /// <summary>
    /// 获取带间隔的属性高度
    /// </summary>
    private static float GetPropertyHeight(SerializedProperty property)
    {
        return EditorGUI.GetPropertyHeight(property, true) + VerticalSpacing;
    }

    /// <summary>
    /// 获取下一行绘制区域
    /// </summary>
    private static Rect GetNextLine(ref Rect rect)
    {
        Rect lineRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        rect.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        return lineRect;
    }

    #endregion
}
#endif
