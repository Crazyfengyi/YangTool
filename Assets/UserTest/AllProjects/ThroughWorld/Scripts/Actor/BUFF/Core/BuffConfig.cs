using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using DataStruct;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// BUFF配置信息
/// </summary>
[Serializable]
public class BuffConfig : ConfigBase
{
    /// <summary>
    /// 名称
    /// </summary>
    public string ConfigName => buffID.ToString();

    [BoxGroup("基本信息"), LabelText("BuffID")]
    public BuffID buffID;

    [BoxGroup("基本信息"), LabelText("描述"), TextArea()]
    public string des;

    [BoxGroup("基本信息"), LabelText("图标"), PreviewField()]
    public Sprite icon;

    [BoxGroup("基本信息"), LabelText("类型")] public BuffType buffType;

    [BoxGroup("基本信息"), LabelText("Flag类型")]
    public BuffFlagType buffFlagType;

    [BoxGroup("基本信息"), LabelText("CD")] public int cd;
    [BoxGroup("基本信息"), LabelText("结束时机")] public BuffEndType buffEndType;
    [BoxGroup("基本信息"), LabelText("时间")] public float time;
    [BoxGroup("分组信息"), LabelText("分组")] public BuffGroupSetting buffGroupSetting;

    [BoxGroup("效果"), LabelText("角色属性更改列表")]
    public List<BuffAttributeChangeInfo> buffAttributeChangeInfo;

    [BoxGroup("特效信息"), LabelText("特效")] public EffectInfo effect;
}