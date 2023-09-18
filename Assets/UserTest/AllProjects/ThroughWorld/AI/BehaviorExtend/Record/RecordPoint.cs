//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "绿点.png")]
//[TaskCategory("RoleAI/Record")]
//[TaskDescription("记录点--有些需要保证目标存在")]
///// <summary>
///// 记录攻击点
///// </summary>
//public class RecordPoint : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 选点类型
//    /// </summary>
//    public RecordPointType recordPointType;
//    /// <summary>
//    /// 选点临时列表
//    /// </summary>
//    private List<Vector3> tempList = new List<Vector3>();
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override TaskStatus OnUpdate()
//    {
//        tempList.Clear();
//        try
//        {
//            switch (recordPointType)
//            {
//                case RecordPointType.记录怪物跳跃到玩家身边的选点:
//                    {
//                        //TODO:先这样
//                        //获取角色范围1的点
//                        //List<Position> list = RoomsMgr.Ins.GetPlayerRangPoint(RoleManager.Ins.Heros[0].transform.position, 1);
//                        //Vector3 result = roleBase.transform.position;

//                        //float minDistance = int.MaxValue;
//                        //for (int i = 0; i < list.Count; i++)
//                        //{
//                        //    Vector3 pos = new Vector3(list[i].x, 0, list[i].z);
//                        //    float distance = Vector3.Distance(roleBase.transform.position, pos);
//                        //    if (distance < minDistance)
//                        //    {
//                        //        minDistance = distance;
//                        //        result = pos;
//                        //    }
//                        //}

//                        Vector3 result = RoleManager.Ins.Heros.Count > 0 ? RoleManager.Ins.Heros[0].transform.position : roleBase.transform.position;
//                        tempList.Add(result);
//                    }
//                    break;
//                case RecordPointType.攻击目标位置:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.Foot).position);
//                    break;
//                case RecordPointType.攻击目标右手位置:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.RightHand).position);
//                    break;
//                case RecordPointType.攻击目标左手位置:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.LeftHand).position);
//                    break;
//                case RecordPointType.攻击目标头部位置:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.Head).position);
//                    break;
//                case RecordPointType.攻击目标身体位置:
//                    tempList.Add(roleBase.GetTarget().roleModelInfo.GetEffectPoint(EffectPointType.Body).position);
//                    break;
//                case RecordPointType.自身位置:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.Foot).position);
//                    break;
//                case RecordPointType.自身左手位置:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.LeftHand).position);
//                    break;
//                case RecordPointType.自身右手位置:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.RightHand).position);
//                    break;
//                case RecordPointType.自身头部位置:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.Head).position);
//                    break;
//                case RecordPointType.自身身体位置:
//                    tempList.Add(roleBase.roleModelInfo.GetEffectPoint(EffectPointType.Body).position);
//                    break;
//                case RecordPointType.记录初始旋转到公共列表:
//                    tempList.Add(roleBase.initialDirection);
//                    break;
//                case RecordPointType.记录远程攻击点_贝塞尔目标点:
//                    tempList.Add(roleBase.GetAtkPointForBessel());
//                    break;
//                default:
//                    tempList.Add(roleBase.transform.position);
//                    Debuger.ToError($"{roleBase.gameObject.name}:选点失败,进入default");
//                    break;
//            }
//        }
//        catch (System.Exception)
//        {
//            Debuger.ToError($"角色:{roleBase.transform.name}的记录点有问题,请先保证有目标");
//            throw;
//        }

//        roleBase.RecordPoints(tempList.ToArray());

//        return TaskStatus.Success;
//    }
//}

///// <summary>
///// 记录点位置
///// </summary>
//public enum RecordPointType
//{
//    None,
//    记录怪物跳跃到玩家身边的选点,
//    攻击目标位置,
//    攻击目标左手位置,
//    攻击目标右手位置,
//    攻击目标头部位置,
//    攻击目标身体位置,
//    自身位置,
//    自身左手位置,
//    自身右手位置,
//    自身头部位置,
//    自身身体位置,
//    记录初始旋转到公共列表,
//    记录远程攻击点_贝塞尔目标点,
//}
