/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-07-30 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using BehaviorDesigner.Runtime;
using UnityEngine.Playables;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System.Resources;
using System.Collections.Generic;
using DataStruct;

/// <summary>
/// 技能控制器
/// </summary>
[Serializable]
public class SkillControl
{
    private RoleBase role;
    private BehaviorTree skillTree;//技能行为树
    private PlayableDirector skillTimeLine;//技能时间线

    public RunTimeSkillData CurrentRunTimeSkillData { get; private set; }
    /// <summary>
    /// 需要使用技能
    /// </summary>
    public bool NeedUseSkill { get; set; }
    /// <summary>
    /// 是否为时间线
    /// </summary>
    public bool IsTimeLine { get; set; }

    public SkillControl(RoleBase _role,BehaviorTree _skillTree, PlayableDirector _timeLine)
    {
        role = _role;
        skillTree = _skillTree;
        skillTimeLine = _timeLine;
    }
    /// <summary>
    /// 使用技能
    /// </summary>
    public void UseSkill(RunTimeSkillData skillData)
    {
        CurrentRunTimeSkillData = skillData;
        NeedUseSkill = true;
        IsTimeLine = CurrentRunTimeSkillData.skill.TimelineORbehavior == 0;

        if (IsTimeLine)
        {
            //TODO:资源加载
            //行为树里正式执行
            skillTimeLine.playableAsset = GameResourceManager.Instance.ResourceLoad<PlayableAsset>($"Skills/{CurrentRunTimeSkillData.skill.ResName}");

            Dictionary<string, PlayableBinding> bindingDict = new Dictionary<string, PlayableBinding>();
            foreach (var at in skillTimeLine.playableAsset.outputs)
            {
                bindingDict.TryAdd(at.streamName, at);
            }

            skillTimeLine.SetGenericBinding(bindingDict["Animation Track"].sourceObject, role.Animator);
        }
    }
    /// <summary>
    /// 打断技能
    /// </summary>
    public void BreakSkill()
    {
        StopSkill();
    }

    void StopSkill()
    {
        BehaviorManager.instance.DisableBehavior(skillTree);
        SkillEnd();
    }
    /// <summary>
    /// 技能结束
    /// </summary>
    public void SkillEnd()
    {
        NeedUseSkill = false;
        CurrentRunTimeSkillData = null;
    }
}