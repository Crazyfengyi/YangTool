//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "Common.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("设置通用bool值Action")]
///// <summary>
///// 设置通用bool值
///// </summary>
//public class CommonSetBool : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// 类型
//    /// </summary>
//    public DataStruct.BoolUseEnum boolUseEnum;

//    /// <summary>
//    /// bool值
//    /// </summary>
//    public bool boolValue;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }
//    public override TaskStatus OnUpdate()
//    {
//        roleBase.SetBoolMark(boolUseEnum, boolValue);
//        return TaskStatus.Success;
//    }
//}
