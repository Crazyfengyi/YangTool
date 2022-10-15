/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-10-15 
*/
using UnityEngine;
using System.Collections;
using System;

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
    [SerializeField]
    private ValueTotal valueTotal;
    /// <summary>
    /// 血条UI
    /// </summary>
    //public HpBarBase heroHealthBar;
    /// <summary>
    /// 死亡回调
    /// </summary>
    private Action dieCallBack;

    public HealthControl(GameActor _gameActor, ValueTotal _value, Action _dieCallBack)
    {
        gameActor = _gameActor;
        valueTotal = _value;
        dieCallBack = _dieCallBack;
    }

    #region 方法
    /// <summary>
    /// 加血
    /// </summary>
    public void AddHp(DamageInfo damageInfo)
    {
        valueTotal.ChangeCurrentValue(Mathf.Abs(damageInfo.damage));
        GameUIManager.Instance.AddScoreShow(gameActor.transform.position, $"{damageInfo.damage}", Color.green);
        //if (heroHealthBar != null)
        //{
        //    heroHealthBar.TakeDamage(GetPercentHp(), Mathf.Abs(damageInfo.damage));
        //}
    }
    /// <summary>
    /// 减血
    /// </summary>
    public void MinusHp(DamageInfo damageInfo)
    {
        valueTotal.ChangeCurrentValue(-Mathf.Abs(damageInfo.damage));
        GameUIManager.Instance.AddScoreShow(gameActor.transform.position, $"-{damageInfo.damage}", Color.red);

        //if (heroHealthBar != null)
        //{
        //    heroHealthBar.TakeDamage(GetPercentHp(), -Mathf.Abs(damageInfo.damage));
        //}

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
    }
    /// <summary>
    /// 设置血条显隐
    /// </summary>
    public void ToogleHpBarShow(bool isShow)
    {
        //if (heroHealthBar != null)
        //{
        //    heroHealthBar.SetActive(isShow);
        //}
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