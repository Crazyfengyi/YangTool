///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-11 
//*/
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// 检测血量
///// </summary>
//public class CheckHpBase : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 比较类型
//    /// </summary>
//    public MathCompare mathCompare = MathCompare.大于;
//    /// <summary>
//    /// 比较值
//    /// </summary>
//    public float targetValue;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (Compare())
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Failure;
//    }

//    public override void OnEnd()
//    {

//    }

//    /// <summary>
//    /// 比较
//    /// </summary>
//    /// <returns>结果</returns>
//    public bool Compare()
//    {
//        //Debug.Log("生命值:" + roleBase.HP.GetCurHp() + " 百分比:" + roleBase.HP.GetPercentHp());

//        switch (mathCompare)
//        {
//            case MathCompare.小于:
//                return roleBase.HPScript.GetCurHp() < targetValue;
//            case MathCompare.等于:
//                return roleBase.HPScript.GetCurHp() == targetValue;
//            case MathCompare.大于:
//                return roleBase.HPScript.GetCurHp() > targetValue;
//            case MathCompare.百分比小于:
//                return roleBase.HPScript.GetPercentHp() < targetValue;
//            case MathCompare.百分比等于:
//                return roleBase.HPScript.GetPercentHp() == targetValue;
//            case MathCompare.百分比大于:
//                return roleBase.HPScript.GetPercentHp() > targetValue;
//            case MathCompare.小于等于:
//                return roleBase.HPScript.GetCurHp() <= targetValue;
//            case MathCompare.大于等于:
//                return roleBase.HPScript.GetCurHp() >= targetValue;
//            case MathCompare.百分比小于等于:
//                return roleBase.HPScript.GetPercentHp() <= targetValue;
//            case MathCompare.百分比大于等于:
//                return roleBase.HPScript.GetPercentHp() >= targetValue;
//            default:
//                Debuger.ToError($"{gameObject.name}的CheakHpBase检测类型未设置");
//                return false;
//        }
//    }
//}

///// <summary>
///// 数学比较
///// </summary>
//public enum MathCompare
//{
//    None,
//    小于,
//    等于,
//    大于,
//    百分比小于,
//    百分比等于,
//    百分比大于,
//    小于等于,
//    大于等于,
//    百分比小于等于,
//    百分比大于等于
//}