//using UnityEngine;
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// ��鷿���Ƿ���
///// </summary>
//public class CheckRoomIsOpen : Conditional
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override TaskStatus OnUpdate()
//    {
//        //if (LayerMgr.Ins.MonsterExistCurRoom((MonsterBase)roleBase))
//        //{
//        //    return TaskStatus.Success;
//        //}
//        return TaskStatus.Failure;
//    }
//}