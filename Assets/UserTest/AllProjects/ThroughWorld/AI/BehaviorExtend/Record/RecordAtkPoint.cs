//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "���.png")]
//[TaskCategory("RoleAI/Record")]
//[TaskDescription("���Լ�¼������,��Ҫ��֤�й���Ŀ��")]
///// <summary>
///// ��¼������
///// </summary>
//public class RecordAtkPoint : Conditional
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// ������λ��
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
//            Debuger.ToError("ѡ��ʧ��");
//        }
//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
