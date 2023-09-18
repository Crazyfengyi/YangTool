//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "Common.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("通过Action")]
///// <summary>
///// 使用当前技能
///// </summary>
//public class CommonAction : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 类型
//    /// </summary>
//    public CommonActionType commonActionType;
//    /// <summary>
//    /// bool值
//    /// </summary>
//    public bool boolValue;
//    /// <summary>
//    /// 数值
//    /// </summary>
//    public float floatVaule;
//    /// <summary>
//    /// 字符串
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
//            case CommonActionType.设置血条显隐:
//                roleBase.SetHpBarShow(boolValue);
//                break;
//            case CommonActionType.设置是否激活:
//                roleBase.SetBoolMark(BoolUseEnum.IsInactive, boolValue);
//                break;
//            case CommonActionType.设置是否攻击中:
//                roleBase.SetBoolMark(BoolUseEnum.IsInAttackTag, boolValue);
//                break;
//            case CommonActionType.设置普攻冷却时间:
//                roleBase.SetFloatMark(DataStruct.FloatUseEnum.AttackCooling, floatVaule);
//                roleBase.SetBoolMark(DataStruct.BoolUseEnum.IsAttackCooling, true);
//                break;
//            case CommonActionType.设置手上武器显隐:
//                if (roleBase as Player)
//                {
//                    ((Player)roleBase).SetHandWeaponShow(boolValue);
//                }
//                else
//                {
//                    roleBase.SetHandWeaponShow(boolValue);
//                }
//                break;
//            case CommonActionType.设置背上武器显隐:
//                roleBase.SetBackWeaponShow(boolValue);
//                break;
//            case CommonActionType.设置技能进入冷却:
//                roleBase.UseCurrentSkill();
//                break;
//            case CommonActionType.玩家专用_设置特殊动作结束:
//                ((Player)roleBase).SetDogeOver();
//                break;
//            case CommonActionType.设置切换职业完成:
//                {
//                    roleBase.SetBoolMark(BoolUseEnum.IsTakeBackWeapon, false);
//                    break;
//                }
//            case CommonActionType.玩家专用_切换职业:
//                //((Player)roleBase).ChangeJob();
//                ((Player)roleBase).SetBoolMark(BoolUseEnum.是否需要切换职业, true);
//                break;
//            case CommonActionType.玩家专用_拿出武器结束:
//                roleBase.SetBoolMark(BoolUseEnum.IsTakeWeapon, false);
//                break;
//            case CommonActionType.玩家专用_需要拿出武器:
//                roleBase.SetBoolMark(BoolUseEnum.IsTakeWeapon, true);
//                break;
//            case CommonActionType.播放特效_跟随:
//                roleBase.PlayEffectAtSelf(strValue, (EffectPointType)floatVaule);
//                break;
//            case CommonActionType.播放特效_不跟随:
//                roleBase.PlayEffect(strValue, (EffectPointType)floatVaule);
//                break;
//            case CommonActionType.清理循环特效:
//                roleBase.ClearAllTempEffect();
//                break;
//            case CommonActionType.设置负面状态:
//                roleBase.SetDeState((DeControlStateType)floatVaule, boolValue);
//                break;
//            case CommonActionType.清理循环音效:
//                roleBase.CleanLoopAudio();
//                break;
//            case CommonActionType.死亡关闭碰撞体:
//                roleBase.DieCloseCollider();
//                break;
//            case CommonActionType.设置滑步打断值:
//                roleBase.SetBoolMark(BoolUseEnum.滑步打断值, boolValue);
//                break;
//            case CommonActionType.宠物专用_跟随主人:
//                ((PetBase)roleBase).SetFlowMaster(boolValue);
//                break;
//            case CommonActionType.设置是否强制跟随中:
//                roleBase.SetBoolMark(BoolUseEnum.强制跟随主人中, boolValue);
//                break;
//            case CommonActionType.激活点:
//                roleBase.ActiveTime();
//                break;
//            case CommonActionType.NULL_占位:
//                {

//                }
//                break;
//            case CommonActionType.死亡动画前:
//                //roleBase.DieFront();
//                break;
//            case CommonActionType.清空临时特效:
//                roleBase.ClearTempEffect();
//                break;
//            default:
//                Debuger.ToError("没识别到通用Action类型");
//                break;
//        }

//        return TaskStatus.Success;
//    }
//}

///// <summary>
///// 通用枚举
///// </summary>
//public enum CommonActionType
//{
//    None,
//    设置血条显隐,
//    设置是否激活,
//    设置是否攻击中,
//    设置普攻冷却时间,
//    设置手上武器显隐,
//    设置背上武器显隐,
//    设置技能进入冷却,
//    玩家专用_设置特殊动作结束,
//    设置切换职业完成,
//    玩家专用_切换职业,
//    玩家专用_拿出武器结束,
//    玩家专用_需要拿出武器,
//    播放特效_跟随,
//    播放特效_不跟随,
//    清理循环特效,
//    设置负面状态,
//    清理循环音效,
//    死亡关闭碰撞体,
//    设置滑步打断值,
//    宠物专用_跟随主人,
//    设置是否强制跟随中,
//    激活点,
//    NULL_占位,
//    死亡动画前,
//    清空临时特效,
//}
