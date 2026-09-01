#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 任务配置中文检视器
/// </summary>
[CustomEditor(typeof(QuestData))]
public sealed class QuestDataEditor : Editor
{
    private const float VerticalSpacing = 2f;

    private SerializedProperty idProperty;
    private SerializedProperty taskTypeProperty;
    private SerializedProperty defaultActiveProperty;
    private SerializedProperty titleProperty;
    private SerializedProperty descriptionProperty;
    private SerializedProperty prerequisiteQuestIdsProperty;
    private SerializedProperty objectivesProperty;
    private SerializedProperty rewardsProperty;
    private ReorderableList prerequisiteQuestIdsList;
    private ReorderableList objectivesList;
    private ReorderableList rewardsList;

    /// <summary>
    /// 初始化任务配置编辑器
    /// </summary>
    private void OnEnable()
    {
        idProperty = serializedObject.FindProperty("Id");
        taskTypeProperty = serializedObject.FindProperty("TaskType");
        defaultActiveProperty = serializedObject.FindProperty("DefaultActive");
        titleProperty = serializedObject.FindProperty("Title");
        descriptionProperty = serializedObject.FindProperty("Description");
        prerequisiteQuestIdsProperty = serializedObject.FindProperty("PrerequisiteQuestIds");
        objectivesProperty = serializedObject.FindProperty("Objectives");
        rewardsProperty = serializedObject.FindProperty("Rewards");
        prerequisiteQuestIdsList = CreateStringList(prerequisiteQuestIdsProperty, "前置任务ID列表", "前置任务ID");
        objectivesList = CreateObjectivesList();
        rewardsList = CreateManagedList(rewardsProperty, typeof(Reward), "任务奖励列表", DrawReward);
    }

    /// <summary>
    /// 绘制任务配置
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(idProperty, new GUIContent("任务ID"));
        EditorGUILayout.PropertyField(taskTypeProperty, new GUIContent("任务类型"));
        EditorGUILayout.PropertyField(defaultActiveProperty, new GUIContent("默认激活任务"));
        EditorGUILayout.PropertyField(titleProperty, new GUIContent("任务标题"));
        EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("任务描述"));
        EditorGUILayout.Space();
        prerequisiteQuestIdsList.DoLayoutList();
        objectivesList.DoLayoutList();
        rewardsList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private ReorderableList CreateStringList(SerializedProperty property, string header, string label)
    {
        ReorderableList list = new ReorderableList(serializedObject, property, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, header);
        list.drawElementCallback = (rect, index, _, _) =>
            EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), new GUIContent($"{label} {index + 1}"));
        return list;
    }

    private ReorderableList CreateObjectivesList()
    {
        ReorderableList list = new ReorderableList(serializedObject, objectivesProperty, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "任务目标列表");
        list.drawElementCallback = (rect, index, _, _) => DrawObjective(rect, objectivesProperty.GetArrayElementAtIndex(index), index);
        list.elementHeightCallback = index => GetObjectiveHeight(objectivesProperty.GetArrayElementAtIndex(index));
        list.onAddCallback = AddDefaultElement;
        return list;
    }

    private ReorderableList CreateManagedList(SerializedProperty property, Type baseType, string header,
        Action<Rect, SerializedProperty, int> drawElement)
    {
        ReorderableList list = new ReorderableList(serializedObject, property, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, header);
        list.drawElementCallback = (rect, index, _, _) => drawElement(rect, property.GetArrayElementAtIndex(index), index);
        list.elementHeightCallback = index => GetManagedElementHeight(property.GetArrayElementAtIndex(index), baseType);
        list.onAddCallback = currentList => ShowAddTypeMenu(currentList, baseType);
        return list;
    }

    private void DrawObjective(Rect rect, SerializedProperty property, int index)
    {
        Rect content = rect;
        property.isExpanded = EditorGUI.Foldout(NextLine(ref content), property.isExpanded, $"任务目标 {index + 1}", true);
        if (!property.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        DrawProperty(ref content, property.FindPropertyRelative("remark"), "备注");
        DrawProperty(ref content, property.FindPropertyRelative("Title"), "目标标题");
        DrawProperty(ref content, property.FindPropertyRelative("Description"), "目标描述");
        DrawProperty(ref content, property.FindPropertyRelative("AutoComplete"), "条件满足后自动完成");
        SerializedProperty condition = property.FindPropertyRelative("Condition");
        content.y += DrawManagedElement(content, condition, typeof(Condition), "条件");
        EditorGUI.indentLevel--;
    }

    private static float GetObjectiveHeight(SerializedProperty property)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        float height = EditorGUIUtility.singleLineHeight + VerticalSpacing;
        height += GetPropertyHeight(property.FindPropertyRelative("remark"));
        height += GetPropertyHeight(property.FindPropertyRelative("Title"));
        height += GetPropertyHeight(property.FindPropertyRelative("Description"));
        height += GetPropertyHeight(property.FindPropertyRelative("AutoComplete"));
        height += GetManagedElementHeight(property.FindPropertyRelative("Condition"), typeof(Condition));
        return height;
    }

    private static void DrawReward(Rect rect, SerializedProperty property, int index)
    {
        DrawManagedElement(rect, property, typeof(Reward), "奖励");
    }

    private static float DrawManagedElement(Rect rect, SerializedProperty property, Type baseType, string label)
    {
        Rect content = rect;
        property.isExpanded = EditorGUI.Foldout(NextLine(ref content), property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        EditorGUI.indentLevel++;
        DrawTypeSelector(ref content, property, baseType);
        DrawSerializedChildren(ref content, property, baseType);
        EditorGUI.indentLevel--;
        return GetManagedElementHeight(property, baseType);
    }

    private static void DrawManagedCondition(Rect rect, SerializedProperty property, int index, Type baseType)
    {
        DrawManagedElement(rect, property, baseType, $"条件 {index + 1}");
    }

    private static float GetManagedElementHeight(SerializedProperty property, Type baseType)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        return EditorGUIUtility.singleLineHeight + VerticalSpacing
               + EditorGUIUtility.singleLineHeight + VerticalSpacing
               + GetSerializedChildrenHeight(property, baseType);
    }

    private static float GetManagedConditionHeight(SerializedProperty property, Type baseType)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        return GetManagedElementHeight(property, baseType);
    }

    private static void DrawTypeSelector(ref Rect rect, SerializedProperty property, Type baseType)
    {
        Rect line = NextLine(ref rect);
        string typeName = property.managedReferenceValue == null
            ? "未选择类型"
            : GetTypeDisplayName(property.managedReferenceValue.GetType());
        if (GUI.Button(new Rect(line.x, line.y, line.width, line.height), $"类型  {typeName}", EditorStyles.popup))
        {
            ShowAddTypeMenu(property, baseType);
        }

        if (property.managedReferenceValue == null)
        {
            EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                "条件或奖励类型为空", MessageType.Warning);
            rect.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }
    }

    private static void DrawSerializedChildren(ref Rect rect, SerializedProperty property, Type baseType)
    {
        if (property.managedReferenceValue == null)
        {
            return;
        }

        SerializedProperty iterator = property.Copy();
        SerializedProperty end = property.GetEndProperty();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            enterChildren = false;
            if (iterator.depth != property.depth + 1 || iterator.name == "Children")
            {
                continue;
            }

            float height = EditorGUI.GetPropertyHeight(iterator, true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, height), iterator, true);
            rect.y += height + VerticalSpacing;
        }

        SerializedProperty children = property.FindPropertyRelative("Children");
        if (children != null && children.isArray)
        {
            EditorGUI.LabelField(NextLine(ref rect), "子条件");
            for (int i = 0; i < children.arraySize; i++)
            {
                SerializedProperty child = children.GetArrayElementAtIndex(i);
                float height = GetManagedConditionHeight(child, typeof(Condition));
                DrawManagedCondition(new Rect(rect.x, rect.y, rect.width, height), child, i, typeof(Condition));
                rect.y += height;
            }

            if (GUI.Button(NextLine(ref rect), "添加子条件"))
            {
                ShowAddTypeMenuForArray(children, typeof(Condition));
            }
        }
    }

    private static float GetSerializedChildrenHeight(SerializedProperty property, Type baseType)
    {
        if (property.managedReferenceValue == null)
        {
            return EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        float height = 0f;
        SerializedProperty iterator = property.Copy();
        SerializedProperty end = property.GetEndProperty();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            enterChildren = false;
            if (iterator.depth == property.depth + 1 && iterator.name != "Children")
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + VerticalSpacing;
            }
        }

        SerializedProperty children = property.FindPropertyRelative("Children");
        if (children != null && children.isArray)
        {
            height += EditorGUIUtility.singleLineHeight + VerticalSpacing;
            for (int i = 0; i < children.arraySize; i++)
            {
                height += GetManagedConditionHeight(children.GetArrayElementAtIndex(i), baseType);
            }

            height += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }

        return height;
    }

    private static void ShowAddTypeMenu(ReorderableList list, Type baseType)
    {
        ShowAddTypeMenuForArray(list.serializedProperty, baseType);
    }

    /// <summary>
    /// 为序列化数组添加一个托管引用元素并显示类型菜单
    /// </summary>
    /// <param name="arrayProperty">托管引用数组属性</param>
    /// <param name="baseType">允许选择的基类</param>
    private static void ShowAddTypeMenuForArray(SerializedProperty arrayProperty, Type baseType)
    {
        if (arrayProperty == null || !arrayProperty.isArray)
        {
            return;
        }

        arrayProperty.serializedObject.Update();
        arrayProperty.arraySize++;
        SerializedProperty element = arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
        element.isExpanded = true;
        arrayProperty.serializedObject.ApplyModifiedProperties();
        ShowAddTypeMenu(element, baseType);
    }

    private static void ShowAddTypeMenu(SerializedProperty property, Type baseType)
    {
        GenericMenu menu = new GenericMenu();
        TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(baseType);
        for (int i = 0; i < types.Count; i++)
        {
            Type type = types[i];
            if (type.IsAbstract || type.ContainsGenericParameters || type.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            menu.AddItem(new GUIContent(GetTypeDisplayName(type)), false, () => SetManagedReference(property, type));
        }

        menu.ShowAsContext();
    }

    /// <summary>
    /// 获取条件或奖励类型的中文名称
    /// </summary>
    /// <param name="type">运行时类型</param>
    /// <returns>显示名称</returns>
    private static string GetTypeDisplayName(Type type)
    {
        if (type == null)
        {
            return "未选择类型";
        }

        switch (type.Name)
        {
            case nameof(ItemNumCondition): return "物品已有数量条件";
            case nameof(CollectCondition): return "收集条件";
            case nameof(AndCondition): return "全部条件";
            case nameof(KillCondition): return "击杀条件";
            case nameof(TalkCondition): return "对话条件";
            case nameof(CustomEventCondition): return "自定义事件条件";
            case nameof(ReachLocationCondition): return "到达地点条件";
            case nameof(AdsCondition): return "广告条件";
            case nameof(TimeCondition): return "时间条件";
            case nameof(ProgressCondition): return "通用进度条件";
            case nameof(OrCondition): return "任一条件";
            case nameof(PassNumCondition): return "通关数量条件";
            case nameof(OnlineTimeCondition): return "在线时长条件";
            case nameof(MoneyReward): return "现金奖励";
            case nameof(GoldReward): return "金币奖励";
            case nameof(ExpReward): return "经验奖励";
            case nameof(ItemReward): return "道具奖励";
            case nameof(CustomReward): return "自定义奖励";
            default: return type.Name;
        }
    }

    private static void SetManagedReference(SerializedProperty property, Type type)
    {
        property.serializedObject.Update();
        property.managedReferenceValue = Activator.CreateInstance(type);
        property.isExpanded = true;
        property.serializedObject.ApplyModifiedProperties();
    }

    private static void AddDefaultElement(ReorderableList list)
    {
        list.serializedProperty.arraySize++;
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
        element.isExpanded = true;
        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }

    private static void DrawProperty(ref Rect rect, SerializedProperty property, string label)
    {
        float height = EditorGUI.GetPropertyHeight(property, true);
        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, height), property, new GUIContent(label), true);
        rect.y += height + VerticalSpacing;
    }

    private static float GetPropertyHeight(SerializedProperty property)
    {
        return EditorGUI.GetPropertyHeight(property, true) + VerticalSpacing;
    }

    private static Rect NextLine(ref Rect rect)
    {
        Rect line = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        rect.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        return line;
    }
}
#endif
