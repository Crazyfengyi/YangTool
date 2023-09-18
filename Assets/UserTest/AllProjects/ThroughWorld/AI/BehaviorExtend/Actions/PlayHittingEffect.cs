//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "effect.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("�ڱ���Ŀ�����ϲ�����Ч")]
///// <summary>
///// ������Ч
///// </summary>
//public class PlayHittingEffect : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// ��Դ����
//    /// </summary>
//    public string name;
//    /// <summary>
//    /// ��Ŀ�����ϲ�
//    /// </summary>
//    public bool isFollow;
//    /// <summary>
//    /// λ��
//    /// </summary>
//    public EffectPointType effectPointType = EffectPointType.Foot;
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }
//    public override TaskStatus OnUpdate()
//    {
//        if (name == null || name == "")
//        {
//            Debuger.ToError($"��ɫ:{roleBase.transform.name}����ЧΪ��");
//            return TaskStatus.Success;
//        }

//        //TODO:��Ч����
//        if (isFollow)
//        {
//            for (int i = 0; i < roleBase.beAttackedList.Count; i++)
//            {
//                if (roleBase.beAttackedList[i])
//                {
//                    roleBase.beAttackedList[i].PlayEffectAtSelf(name, effectPointType);
//                }
//            }
//        }
//        else
//        {
//            for (int i = 0; i < roleBase.beAttackedList.Count; i++)
//            {
//                if (roleBase.beAttackedList[i])
//                {
//                    roleBase.beAttackedList[i].PlayEffect(name, effectPointType);
//                }
//            }
//        }

//        roleBase.beAttackedList.Clear();

//        return TaskStatus.Success;
//    }
//}
