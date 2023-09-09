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
using YangTools.UGUI;
using BehaviorDesigner.Runtime;
using UnityEngine.Playables;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System.Resources;

[Serializable]
/// <summary>
/// 技能控制器
/// </summary>
public class SkillControl
{
    private BehaviorTree skillTree;//技能Tree
    private PlayableDirector timeLine;//技能

    protected bool needUseSkill;
    /// <summary>
    /// 技能树是否需要使用技能
    /// </summary>
    public bool SkillTreeNeedUseSkill
    {
        get
        {
            return needUseSkill;
        }
        set
        {
            needUseSkill = value;
        }
    }

    public SkillControl(BehaviorTree _skillTree, PlayableDirector _timeLine)
    {
        skillTree = _skillTree;
        skillTree.OnBehaviorEnd += SkillEnd;

        timeLine = _timeLine;
    }
    /// <summary>
    /// 使用技能
    /// </summary>
    public void UseSkill(string skillName,bool isTimeLine = false)
    {
        needUseSkill = true;
        if (isTimeLine)
        {
            //TODO:测试用,正式应该用行为树里执行
            timeLine.playableAsset = GameResourceManager.Instance.ResoruceLoad<PlayableAsset>($"Skills/{skillName}");
            timeLine.Play();
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
    }
    /// <summary>
    /// 技能结束
    /// </summary>
    void SkillEnd(Behavior behavior)
    {

    }
}