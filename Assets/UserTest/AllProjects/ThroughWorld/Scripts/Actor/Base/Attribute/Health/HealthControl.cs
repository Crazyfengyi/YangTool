/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-10-15 
*/
using System;
using DataStruct;
using UnityEngine;
using YangTools.Scripts.Core.YangExtend;

[Serializable]
public class HealthControl
{
    /// <summary>
    /// 游戏表演者
    /// </summary>
    private GameActor gameActor;

    /// <summary>
    /// 血量值
    /// </summary>
    [SerializeField] private ValueTotal valueTotal;

    // <summary>
    // 血条UI
    // </summary>
    public HPBarObjectPoolItem healthBar;

    /// <summary>
    /// 死亡回调
    /// </summary>
    private Action dieCallBack;

    public HealthControl(GameActor _gameActor, ValueTotal _value, Action _dieCallBack)
    {
        gameActor = _gameActor;
        valueTotal = _value;
        dieCallBack = _dieCallBack;
        Create();
    }

    /// <summary>
    /// 异步创建角色血条并处理创建期间角色被销毁的情况
    /// </summary>
    public async void Create()
    {
        if (!gameActor)
        {
            return;
        }

        Transform hpPoint = gameActor.transform.Find("HPBarPoint");
        healthBar = await GameUIManager.Instance.GetHPBar(hpPoint ?? gameActor.transform, "血条");
        if (!gameActor && healthBar != null)
        {
            GameUIManager.Instance.RecycleHPBar(healthBar);
            healthBar = null;
        }
    }

#region 方法
    /// <summary>
    /// 加血
    /// </summary>
    public void AddHp(DamageInfo damageInfo)
    {
        valueTotal.ChangeCurrentValue(Mathf.Abs(damageInfo.damage));
        GameUIManager.Instance.AddScoreShow(gameActor.transform.position, $"{damageInfo.damage}", Color.green);
        healthBar.UpdateData(gameActor);
    }
    /// <summary>
    /// 减血
    /// </summary>
    public void MinusHp(DamageInfo damageInfo)
    {
        valueTotal.ChangeCurrentValue(-Mathf.Abs(damageInfo.damage));
        Vector3 atkPos = damageInfo.behitPos == default ? gameActor.transform.position : damageInfo.behitPos;

        GameUIManager.Instance.AddScoreShow(atkPos, $"-{damageInfo.damage}", "#EE9A00".GetColor());
        healthBar.UpdateData(gameActor);

        if ((int)valueTotal.Value < 1)
        {
            //if (gameActor.GetBoolMark(BoolUseEnum.IsDie))
            //{
            //    Debuger.ToWarning("目标已经死亡,无法再次死亡");
            //    return;
            //}
            DieEvent();
        }
    }
    /// <summary>
    /// 死亡事件
    /// </summary>
    public void DieEvent()
    {
        dieCallBack?.Invoke();
        RecycleHealthBar();
    }
    /// <summary>
    /// 场景切换删除  
    /// </summary>
    public void IDestroy()
    {
        RecycleHealthBar();
    }

    /// <summary>
    /// 回收血条并清除当前引用
    /// </summary>
    private void RecycleHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        GameUIManager.Instance.RecycleHPBar(healthBar);
        healthBar = null;
    }
    /// <summary>
    /// 设置血条显隐
    /// </summary>
    public void ToogleHpBarShow(bool isShow)
    {
        healthBar?.ToogleHPBar(isShow);
    }
    #endregion

    #region 获取值
    /// <summary>
    /// 获得当前血量
    /// </summary>
    public int GetCurrentHp()
    {
        return (int)valueTotal.Value;
    }
    /// <summary>
    /// 获得百分比血量
    /// </summary>
    /// <returns>0~1</returns>
    public float GetPercentHp()
    {
        return valueTotal.Percent;
    }
    #endregion
}
