//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "攻击.png")]
//[TaskCategory("RoleAI/Atk")]
//[TaskDescription("需要先记录攻击点")]
///// <summary>
///// 范围伤害
///// </summary>
//public class AreaDamage : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 范围类型
//    /// </summary>
//    public HitCheckType areaDamageType;
//    /// <summary>
//    /// 范围(圆形:半径 矩形:长 伞形:直径)
//    /// </summary>
//    public float rang = 10f;
//    /// <summary>
//    /// 角度(扇形:角度)
//    /// </summary>
//    public float angle = 0f;
//    /// <summary>
//    /// 偏移量
//    /// </summary>
//    public Vector3 OffsetVector;
//    /// <summary>
//    /// 旋转
//    /// </summary>
//    public Vector3 Rotate;
//    /// <summary>
//    /// 击中特效
//    /// </summary>
//    public string hitEffectName;
//    /// <summary>
//    /// 击中声音
//    /// </summary>
//    public string hitSoundName;
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override TaskStatus OnUpdate()
//    {

//        switch (areaDamageType)
//        {
//            case HitCheckType.圆形:
//                roleBase.CircleRangAtk(roleBase.recordPointList[0], rang, OffsetVector, Rotate, hitEffectName, hitSoundName);
//                break;
//            case HitCheckType.扇形:
//                roleBase.SectorRangAtk(roleBase.recordPointList[0], rang, angle, OffsetVector, Rotate, hitEffectName, hitSoundName);
//                break;
//            case HitCheckType.矩形:
//                roleBase.RectRangAtk(roleBase.recordPointList[0], rang, angle, OffsetVector, Rotate, hitEffectName, hitSoundName);
//                break;
//            default:
//                break;
//        }

//        return TaskStatus.Success;
//    }
//}
