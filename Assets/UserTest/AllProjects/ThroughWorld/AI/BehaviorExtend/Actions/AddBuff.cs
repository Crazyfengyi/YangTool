//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "����.jpeg")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("��Buff Action")]
///// <summary>
///// ʹ�õ�ǰ����
///// </summary>
//public class AddBuff : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// ��Դ����
//    /// </summary>
//    public BuffID buffId;

//    /// <summary>
//    /// ��������
//    /// </summary>
//    public int percent = 50;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        if(Random.Range(1, 101) <= percent)
//        {
//            foreach (RoleBase rb in this.roleBase.beAttackedList)
//            {
//                rb.AddBuff(buffId);
//            }
//        }

//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
