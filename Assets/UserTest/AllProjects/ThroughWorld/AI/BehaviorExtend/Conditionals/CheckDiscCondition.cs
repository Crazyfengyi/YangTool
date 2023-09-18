///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       John 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-07-16 
//*/
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// 检测最近敌人的距离
///// </summary>
//public class CheckDiscContion : Conditional
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
//        RoleBase target =  roleBase.GetTarget();
//        float disc = (target.transform.position - roleBase.transform.position).magnitude;
//        Debug.Log("比较距离: disc:" + disc);
//        switch (mathCompare)
//        {
//            case MathCompare.小于:
//                return disc < targetValue;
//            case MathCompare.等于:
//                return disc == targetValue;
//            case MathCompare.大于:
//                return disc > targetValue;
//            case MathCompare.百分比小于:
//                return disc < targetValue;
//            case MathCompare.百分比等于:
//                return disc == targetValue;
//            case MathCompare.百分比大于:
//                return disc > targetValue;
//            case MathCompare.小于等于:
//                return disc <= targetValue;
//            case MathCompare.大于等于:
//                return disc >= targetValue;
//            case MathCompare.百分比小于等于:
//                return disc <= targetValue;
//            case MathCompare.百分比大于等于:
//                return disc >= targetValue;
//            default:
//                Debuger.ToError($"{gameObject.name}的CheakHpBase检测类型未设置");
//                return false;
//        }
//    }
//}