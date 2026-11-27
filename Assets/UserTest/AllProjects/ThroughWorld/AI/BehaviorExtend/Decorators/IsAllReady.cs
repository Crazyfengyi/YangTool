using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 按顺序运行子节点且全部成功时通过
/// </summary>
[TaskIcon(BehaviorSetting.path + "allOK.png")]
[TaskCategory("RoleAI")]
[TaskDescription("按顺序运行子节点且全部成功时通过")]
public class IsAllReady : Sequence
{

}
