//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[TaskIcon(BehaviorSetting.path + "标识_叹号.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("矩形警告区域Action")]
///// <summary>
///// 播放特效
///// </summary>
//public class PlayWarningRectangle : Action
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
//    /// 长度
//    /// </summary>
//    public float length = 5f;
//    /// <summary>
//    /// 宽度
//    /// </summary>
//    public float width = 2;

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

//        float len = length / 4 + 1;

//        //特效
//        GameObject effectPrefabs = Resources.Load<GameObject>($"矩形预警");
//        GameObject effectObj = GameObject.Instantiate(effectPrefabs, nowPos, Quaternion.identity);
//        effectObj.transform.forward = roleBase.modelInfo.Root.transform.forward;
//        //缩放
//        Vector3 resultScale = Vector3.one;
//        resultScale.z *= len;
//        resultScale.x *= width;
//        effectObj.transform.localScale = resultScale;

//        return TaskStatus.Success;
//    }
//}
