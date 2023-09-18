//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "effect.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("播放特效Action")]
///// <summary>
///// 播放特效
///// </summary>
//public class PlayEffect : Action
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
//    /// 自己身上
//    /// </summary>
//    public bool isSelf;
//    /// <summary>
//    /// 位置
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
//            Debuger.ToError($"角色:{roleBase.transform.name}的特效为空");
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
