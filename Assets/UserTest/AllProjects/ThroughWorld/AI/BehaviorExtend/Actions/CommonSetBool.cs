//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "Common.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("����ͨ��boolֵAction")]
///// <summary>
///// ����ͨ��boolֵ
///// </summary>
//public class CommonSetBool : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// ����
//    /// </summary>
//    public DataStruct.BoolUseEnum boolUseEnum;

//    /// <summary>
//    /// boolֵ
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
