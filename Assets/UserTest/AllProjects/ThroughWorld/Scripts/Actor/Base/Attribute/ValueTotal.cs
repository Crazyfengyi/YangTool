/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-21 
*/
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 总值
/// </summary>
[Serializable]
public class ValueTotal
{
    //第一层时Core（核心层）。
    //Core层是玩家各个其他模块的属性总和
    [ShowInInspector]
    [SerializeField]
    private float value;//当前值
    public float Value
    {
        get => value;
        private set => this.value = value;
    }

    private float baseValue = 0;//基础值 
    public float BaseValue => baseValue;

    private float addValue = 0;//增加值
    private float addPercent = 0;//百分比增加值
    private float finalAdd = 0;//最终增加
    private float finalAddPercent = 0;//最终百分比
    private float maxValue = 0;//最大值

    //第二层是External(外部层),Buff修改属性的总和。
    //两者相加既为玩家的实时属性。
    //下面是BUFF系统的数值修饰器
    private ModifierCollector AddCollector { get; } = new ModifierCollector();
    private ModifierCollector AddPrencentCollector { get; } = new ModifierCollector();
    private ModifierCollector FinalAddCollector { get; } = new ModifierCollector();
    private ModifierCollector FinalAddPercentCollector { get; } = new ModifierCollector();

    /// <summary>
    /// 百分比
    /// </summary>
    public float Percent => Value / maxValue;
    /// <summary>
    /// 计算公式
    /// </summary>
    public Func<ValueTotal, float> mathFun;
    /// <summary>
    /// 重设最大值
    /// </summary>
    public Action<float> maxReset;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="baseVal">基础值</param>
    /// <param name="addVal">加成值</param>
    /// <param name="proVal">加成比例</param>
    /// <param name="totalProVal">整体加成比例</param>
    /// <param name="_mathFun">使用的计算公式</param>
    /// <param name="_maxReset">重设最大值回调</param>
    public ValueTotal(float _baseValue = 0, float _addValue = 0, float _addPercent = 0, float _finalAdd = 0, float _finalAddPercent = 0, Func<ValueTotal, float> _mathFun = null, Action<float> _maxReset = null)
    {
        baseValue = _baseValue;
        addValue = _addValue;
        addPercent = _addPercent;
        finalAdd = _finalAdd;
        finalAddPercent = _finalAddPercent;
        mathFun = _mathFun;
        maxReset = _maxReset;
        ComputeTotalValue();
    }
    /// <summary>
    /// 计算总值
    /// </summary>
    public void ComputeTotalValue()
    {
        ComputeMaxValue(false);
        Value = maxValue;
    }
    /// <summary>
    /// 计算最大值
    /// </summary>
    public void ComputeMaxValue(bool isChangeValue = true)
    {
        float oldMaxValue = maxValue;
        if (mathFun != null)
        {
            //计算公式
            maxValue = mathFun.Invoke(this);
        }
        else
        {
            //默认计算公式
            var value1 = (baseValue + addValue + AddCollector.TotalValue) * (1 + addPercent + AddPrencentCollector.TotalValue);
            maxValue = (value1 + finalAdd + FinalAddCollector.TotalValue) * (1 + finalAddPercent + FinalAddPercentCollector.TotalValue);
        }
        if (isChangeValue)
        {
            ChangeCurrentValue(maxValue - oldMaxValue);
        }

        maxReset?.Invoke(maxValue);
    }
    /// <summary>
    /// 改变当前值(受上限限制)
    /// </summary>
    public void ChangeCurrentValue(float value)
    {
        Value += value;
        Value = Mathf.Clamp(Value, 0, maxValue);
    }
    /// <summary>
    /// 更改值
    /// </summary>
    public void AddValue(AttributeValueType valueType, float value, bool isChangeCurValue = true)
    {
        switch (valueType)
        {
            case AttributeValueType.BaseValue:
                baseValue += value;
                break;
            case AttributeValueType.AddValue:
                addValue += value;
                break;
            case AttributeValueType.AddPercent:
                addPercent += value;
                break;
            case AttributeValueType.FinalAdd:
                finalAdd += value;
                break;
            case AttributeValueType.FinalAddPercent:
                finalAddPercent += value;
                break;
            default:
                break;
        }
        ComputeMaxValue(isChangeCurValue);
    }
    /// <summary>
    /// 克隆
    /// </summary>
    public ValueTotal Clone()
    {
        ValueTotal attribute = new ValueTotal(baseValue, addValue, addPercent, finalAddPercent);
        attribute.Value = Value;
        attribute.maxValue = maxValue;
        return attribute;
    }
}

/// <summary>
/// 修饰器控制器
/// </summary>
public class ModifierCollector
{
    public float TotalValue { get; private set; }
    private List<ValueModifier> Modifiers { get; } = new List<ValueModifier>();
    public float AddModifier(ValueModifier modifier)
    {
        Modifiers.Add(modifier);
        Update();
        return TotalValue;
    }
    public float RemoveModifier(ValueModifier modifier)
    {
        Modifiers.Remove(modifier);
        Update();
        return TotalValue;
    }
    public void Update()
    {
        TotalValue = 0;
        foreach (var item in Modifiers)
        {
            TotalValue += item.Value;
        }
    }
}
/// <summary>
/// 值修饰器
/// </summary>
public class ValueModifier
{
    private float _value;
    public float Value
    {
        get { return _value; }
        set
        {
            _value = value;
            //发送值更改事件,更新属性
        }
    }
}

/// <summary>
/// 属性值类型
/// </summary>
public enum AttributeValueType
{
    None,
    BaseValue,
    AddValue,
    AddPercent,
    FinalAdd,
    FinalAddPercent,
}