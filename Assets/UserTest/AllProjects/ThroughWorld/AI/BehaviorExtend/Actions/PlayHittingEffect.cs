//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "effect.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("在被击目标身上播放特效")]
///// <summary>
///// 播放特效
///// </summary>
//public class PlayHittingEffect : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 资源名字
//    /// </summary>
//    public string name;
//    /// <summary>
//    /// 在目标身上播
//    /// </summary>
//    public bool isFollow;
//    /// <summary>
//    /// 位置
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
//            Debuger.ToError($"角色:{roleBase.transform.name}的特效为空");
//            return TaskStatus.Success;
//        }

//        //TODO:特效管理
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
