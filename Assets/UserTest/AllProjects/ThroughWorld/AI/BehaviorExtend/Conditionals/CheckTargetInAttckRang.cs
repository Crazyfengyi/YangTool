//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon("Assets/0.Game/_Assets/CoreScript/Behavior_Extend/Gizmos/IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// ����Ƿ���Ŀ��
///// </summary>
//public class CheckTargetInAttckRang : Conditional
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
//        //Debuger.ToError($"�Ƿ��ڹ�����Χ��:{roleBase.CheckTargetInAttackRange()}");

//        if (roleBase.CheckTargetInAttackRange())
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Failure;
//    }

//    public override void OnEnd()
//    {

//    }
//}
