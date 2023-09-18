//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "标识_叹号.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("圆形警告区域Action")]
///// <summary>
///// 播放特效
///// </summary>
//public class PlayWarningCircular : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// 持续时间
//    /// </summary>
//    public float time = 1;

//    /// <summary>
//    /// 半径
//    /// </summary>
//    public float radius = 5f;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override TaskStatus OnUpdate()
//    {
//        //目标点
//        Vector3 targetPos = roleBase.recordPointList[0];
//        targetPos.y = 0.002f;

//        //自身位置
//        Vector3 nowPos = transform.position;
//        nowPos.y = 0.002f;
//        Vector3 direction = targetPos - nowPos;

//        //特效
//        GameObject effectPrefabs = Resources.Load<GameObject>($"圆形预警");
//        GameObject effectObj = GameObject.Instantiate(effectPrefabs, targetPos, Quaternion.identity);
//        effectObj.transform.localScale = Vector3.one * radius;
//        effectObj.GetComponent<DelayDestory>().InitAndStart(time);

//        return TaskStatus.Success;
//    }
//}
