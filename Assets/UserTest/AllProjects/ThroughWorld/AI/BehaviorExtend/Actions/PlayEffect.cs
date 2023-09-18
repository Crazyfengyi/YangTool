//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "effect.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("������ЧAction")]
///// <summary>
///// ������Ч
///// </summary>
//public class PlayEffect : Action
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
//    /// �Լ�����
//    /// </summary>
//    public bool isSelf;
//    /// <summary>
//    /// λ��
//    /// </summary>
//    public EffectPointType effectPointType;
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

//        if (isSelf)
//        {
//            GameObject effect = roleBase.PlayEffectAtSelf(name, effectPointType);
//            roleBase.AddTempEffect(effect);
//        }
//        else
//        {
//            GameObject effect = roleBase.PlayEffect(name, effectPointType);
//            roleBase.AddTempEffect(effect);
//        }

//        return TaskStatus.Success;
//    }
//}
