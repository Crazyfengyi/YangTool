//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "Common.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("ͨ��Action")]
///// <summary>
///// ʹ�õ�ǰ����
///// </summary>
//public class CommonAction : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// ����
//    /// </summary>
//    public CommonActionType commonActionType;
//    /// <summary>
//    /// boolֵ
//    /// </summary>
//    public bool boolValue;
//    /// <summary>
//    /// ��ֵ
//    /// </summary>
//    public float floatVaule;
//    /// <summary>
//    /// �ַ���
//    /// </summary>
//    public string strValue;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }
//    public override TaskStatus OnUpdate()
//    {
//        switch (commonActionType)
//        {
//            case CommonActionType.����Ѫ������:
//                roleBase.SetHpBarShow(boolValue);
//                break;
//            case CommonActionType.�����Ƿ񼤻�:
//                roleBase.SetBoolMark(BoolUseEnum.IsInactive, boolValue);
//                break;
//            case CommonActionType.�����Ƿ񹥻���:
//                roleBase.SetBoolMark(BoolUseEnum.IsInAttackTag, boolValue);
//                break;
//            case CommonActionType.�����չ���ȴʱ��:
//                roleBase.SetFloatMark(DataStruct.FloatUseEnum.AttackCooling, floatVaule);
//                roleBase.SetBoolMark(DataStruct.BoolUseEnum.IsAttackCooling, true);
//                break;
//            case CommonActionType.����������������:
//                if (roleBase as Player)
//                {
//                    ((Player)roleBase).SetHandWeaponShow(boolValue);
//                }
//                else
//                {
//                    roleBase.SetHandWeaponShow(boolValue);
//                }
//                break;
//            case CommonActionType.���ñ�����������:
//                roleBase.SetBackWeaponShow(boolValue);
//                break;
//            case CommonActionType.���ü��ܽ�����ȴ:
//                roleBase.UseCurrentSkill();
//                break;
//            case CommonActionType.���ר��_�������⶯������:
//                ((Player)roleBase).SetDogeOver();
//                break;
//            case CommonActionType.�����л�ְҵ���:
//                {
//                    roleBase.SetBoolMark(BoolUseEnum.IsTakeBackWeapon, false);
//                    break;
//                }
//            case CommonActionType.���ר��_�л�ְҵ:
//                //((Player)roleBase).ChangeJob();
//                ((Player)roleBase).SetBoolMark(BoolUseEnum.�Ƿ���Ҫ�л�ְҵ, true);
//                break;
//            case CommonActionType.���ר��_�ó���������:
//                roleBase.SetBoolMark(BoolUseEnum.IsTakeWeapon, false);
//                break;
//            case CommonActionType.���ר��_��Ҫ�ó�����:
//                roleBase.SetBoolMark(BoolUseEnum.IsTakeWeapon, true);
//                break;
//            case CommonActionType.������Ч_����:
//                roleBase.PlayEffectAtSelf(strValue, (EffectPointType)floatVaule);
//                break;
//            case CommonActionType.������Ч_������:
//                roleBase.PlayEffect(strValue, (EffectPointType)floatVaule);
//                break;
//            case CommonActionType.����ѭ����Ч:
//                roleBase.ClearAllTempEffect();
//                break;
//            case CommonActionType.���ø���״̬:
//                roleBase.SetDeState((DeControlStateType)floatVaule, boolValue);
//                break;
//            case CommonActionType.����ѭ����Ч:
//                roleBase.CleanLoopAudio();
//                break;
//            case CommonActionType.�����ر���ײ��:
//                roleBase.DieCloseCollider();
//                break;
//            case CommonActionType.���û������ֵ:
//                roleBase.SetBoolMark(BoolUseEnum.�������ֵ, boolValue);
//                break;
//            case CommonActionType.����ר��_��������:
//                ((PetBase)roleBase).SetFlowMaster(boolValue);
//                break;
//            case CommonActionType.�����Ƿ�ǿ�Ƹ�����:
//                roleBase.SetBoolMark(BoolUseEnum.ǿ�Ƹ���������, boolValue);
//                break;
//            case CommonActionType.�����:
//                roleBase.ActiveTime();
//                break;
//            case CommonActionType.NULL_ռλ:
//                {

//                }
//                break;
//            case CommonActionType.��������ǰ:
//                //roleBase.DieFront();
//                break;
//            case CommonActionType.�����ʱ��Ч:
//                roleBase.ClearTempEffect();
//                break;
//            default:
//                Debuger.ToError("ûʶ��ͨ��Action����");
//                break;
//        }

//        return TaskStatus.Success;
//    }
//}

///// <summary>
///// ͨ��ö��
///// </summary>
//public enum CommonActionType
//{
//    None,
//    ����Ѫ������,
//    �����Ƿ񼤻�,
//    �����Ƿ񹥻���,
//    �����չ���ȴʱ��,
//    ����������������,
//    ���ñ�����������,
//    ���ü��ܽ�����ȴ,
//    ���ר��_�������⶯������,
//    �����л�ְҵ���,
//    ���ר��_�л�ְҵ,
//    ���ר��_�ó���������,
//    ���ר��_��Ҫ�ó�����,
//    ������Ч_����,
//    ������Ч_������,
//    ����ѭ����Ч,
//    ���ø���״̬,
//    ����ѭ����Ч,
//    �����ر���ײ��,
//    ���û������ֵ,
//    ����ר��_��������,
//    �����Ƿ�ǿ�Ƹ�����,
//    �����,
//    NULL_ռλ,
//    ��������ǰ,
//    �����ʱ��Ч,
//}
