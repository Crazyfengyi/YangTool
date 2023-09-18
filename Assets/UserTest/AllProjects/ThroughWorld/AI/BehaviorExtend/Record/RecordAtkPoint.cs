//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "红点.png")]
//[TaskCategory("RoleAI/Record")]
//[TaskDescription("尝试记录攻击点,需要保证有攻击目标")]
///// <summary>
///// 记录攻击点
///// </summary>
//public class RecordAtkPoint : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 攻击点位置
//    /// </summary>
//    public EffectPointType attackPointType = EffectPointType.Body;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (roleBase.GetTarget())
//        {
//            roleBase.atkPoint = roleBase.GetTarget().roleModelInfo.GetEffectPoint(attackPointType).position;
//        }
//        else
//        {
//            roleBase.atkPoint = roleBase.transform.position;
//            Debuger.ToError("选点失败");
//        }
//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
