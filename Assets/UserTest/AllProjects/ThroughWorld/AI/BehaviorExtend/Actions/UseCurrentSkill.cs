//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("ʹ�õ�ǰ����")]
///// <summary>
///// ʹ�õ�ǰ����
///// </summary>
//public class UseCurrentSkill : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// �Ƿ�����������ȴ
//    /// </summary>
//    public bool isImmediatelyCooling;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        roleBase.UseCurrentSkill(isImmediatelyCooling);

//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
