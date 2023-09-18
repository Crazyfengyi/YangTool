//using UnityEngine;
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// 检查房间是否开启
///// </summary>
//public class CheckRoomIsOpen : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
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