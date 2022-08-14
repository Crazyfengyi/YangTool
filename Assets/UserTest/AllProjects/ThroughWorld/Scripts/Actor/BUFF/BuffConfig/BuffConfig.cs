using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
/// <summary>
/// BUFF配置信息
/// </summary>
public class BuffConfig : ConfigBase
{
    /// <summary>
    /// 名称
    /// </summary>
    public string ConfigName
    {
        get
        {
            return buffID.ToString();
        }
    }
    [BoxGroup("基本信息"), LabelText("BuffID")]
    public BuffID buffID;
    [BoxGroup("基本信息"), LabelText("描述"), TextArea()]
    public string des;
    [BoxGroup("基本信息"), LabelText("图标"), PreviewField()]
    public Texture2D icon;
    [BoxGroup("基本信息"), LabelText("类型")]
    public BuffType buffType;
    [BoxGroup("基本信息"), LabelText("CD")]
    public int CD;
    [BoxGroup("基本信息"), LabelText("结束时机")]
    public BuffEndType buffEndType;
    [BoxGroup("基本信息"), LabelText("时间")]
    public float time;
    [BoxGroup("分组信息"), LabelText("分组")]
    public BuffGroupSetting buffGroupSetting;

    [BoxGroup("特效信息"), LabelText("特效")]
    public EffectInfo effect;
}

