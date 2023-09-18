//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "Common.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("设置通用Float值Action")]
///// <summary>
///// 设置通用Float值
///// </summary>
//public class CommonSetFloat : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 类型
//    /// </summary>
//    public FloatUseEnum floatUseEnum;
//    /// <summary>
//    /// float值
//    /// </summary>
//    public float floatValue;
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }
//    public override TaskStatus OnUpdate()
//    {
//        roleBase.SetFloatMark(floatUseEnum, floatValue);
//        return TaskStatus.Success;
//    }
//}
