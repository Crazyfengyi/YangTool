//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// ����Ƿ���Ŀ��
///// </summary>
//public class CheckIsHaveTarget : Conditional
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;

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
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Failure;
//    }

//    public override void OnEnd()
//    {
       
//    }
//}
