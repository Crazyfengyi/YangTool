//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "Common.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("����ͨ��FloatֵAction")]
///// <summary>
///// ����ͨ��Floatֵ
///// </summary>
//public class CommonSetFloat : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// ����
//    /// </summary>
//    public FloatUseEnum floatUseEnum;
//    /// <summary>
//    /// floatֵ
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
