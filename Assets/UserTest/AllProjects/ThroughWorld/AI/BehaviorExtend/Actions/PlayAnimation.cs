/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-05-11 
*/
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
using System.Reflection;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using YangTools.Log;
using YangTools.Extend;
using Sirenix.OdinInspector;
using UnityEngine.Diagnostics;
using DataStruct;

//[BehaviorDesigner.Runtime.Tasks.HelpURL("http://www.baidu.com")]
[TaskIcon(BehaviorSetting.path + "AniAndEventIcon.png")]
[TaskCategory("RoleAI/Animator")]
[TaskDescription("注意:动画事件倒序遍历(同一时间点,后加先执行)")]
/// <summary>
/// 播放动画
/// </summary>
public class PlayAnimation : Action
{
    /// <summary>
    /// 自身角色脚本
    /// </summary>
    private RoleBase roleBase;

    /// <summary>
    /// 动画名称
    /// </summary>
    public string aniName;
    /// <summary>
    /// 立即退出
    /// </summary>
    public bool ExitImmediately;

    /// <summary>
    /// 攻速依赖
    /// </summary>
    public AtkSpeedDepend depend;
    /// <summary>
    /// 攻速依赖百分比
    /// </summary>
    public float dependPercent;
    /// <summary>
    /// 攻速依赖值
    /// </summary>
    public float dependValue;

    /// <summary>
    /// 事件信息
    /// </summary>
    public List<AniEventInfo> aniEventInfos = new List<AniEventInfo>();

    //动画速度缩放
    private float aniScale;
    //时间
    private float timer;
    //动画片段长度
    private float aniMaxTime;
    [ShowInInspector]
    //运行时拿到的动画
    private AnimationClip resultClipInfo;

    public override void OnAwake()
    {
        roleBase = gameObject.GetComponent<RoleBase>();
        GetAniEvent();

        try
        {
            //获取动画片段
            resultClipInfo = roleBase.Animator.GetAnimClipByName(aniName);
        }
        catch (Exception)
        {
            Debuger.ToError($"{roleBase.transform.name}角色:获取{aniName}动画出错");
            throw;
        }
    }

    private void GetAniEvent()
    {
        #region 动画事件反射
        for (int i = 0; i < aniEventInfos.Count; i++)
        {
            aniEventInfos[i].targetGameObject = gameObject;
            AniEventInfo info = aniEventInfos[i];

            //目标脚本(组件)
            Type type = TaskUtility.GetTypeWithinAssembly(info.componentName);
            if (type == null)
            {
                Debuger.ToError($"{gameObject.name}:无法调用-类型{info.componentName}为空");
            }
            //获得脚本
            Component component = GetDefaultGameObject(info.targetGameObject).GetComponent(type);
            if (component == null)
            {
                Debuger.ToError($"{gameObject.name}的脚本{info.componentName}为空,不能调用");
            }

            //方法
            MethodInfo method = component.GetType().GetMethod(info.methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //方法参数
            ParameterInfo[] parmaInfo = method.GetParameters();
            //参数类型列表
            List<Type> parameterTypeList = new List<Type>();
            //参数数组
            object[] parmaArray = new object[parmaInfo.Length];
            //临时参数
            string tempParma = string.Empty;
            string[] parmeterList = info.parmeterString.Split('|');

            for (int j = 0; j < parmaInfo.Length; j++)
            {
                if (j >= parmeterList.Length || string.IsNullOrEmpty(parmeterList[j]))
                {
                    if (parmaInfo[j].HasDefaultValue)
                    {
                        //用默认值
                        parmaArray[j] = parmaInfo[j].DefaultValue;
                        parameterTypeList.Add(parmaInfo[j].ParameterType);
                        continue;
                    }
                    else
                    {
                        Debuger.ToError($"{roleBase.transform.name}角色:动画{aniName}的事件:{method}第{j}个参数缺失,请填写所有参数!");
                        throw null;
                    }
                }
                else
                {
                    tempParma = parmeterList[j];
                }

                switch (Type.GetTypeCode(parmaInfo[j].ParameterType))
                {
                    case TypeCode.Int32:
                        parmaArray[j] = int.Parse(tempParma);
                        break;
                    case TypeCode.Single:
                        parmaArray[j] = float.Parse(tempParma);
                        break;
                    case TypeCode.Boolean:
                        parmaArray[j] = bool.Parse(tempParma);
                        break;
                    case TypeCode.String:
                        parmaArray[j] = tempParma;
                        break;
                    case TypeCode.Int64:
                        parmaArray[j] = long.Parse(tempParma);
                        break;
                    default:
                        Debuger.ToError("AniAndEvent中参数只支持int,float,string,bool");
                        break;
                }
                parameterTypeList.Add(parmaInfo[j].ParameterType);
            }

            //获得方法
            MethodInfo methodInfo = component.GetType().GetMethod(info.methodName, parameterTypeList.ToArray());

            if (methodInfo == null)
            {
                Debuger.ToError($"脚本{info.componentName}上方法{info.methodName}未找到,参数列表{parameterTypeList.ToArray()}");
            }

            info.component = component;
            info.methodInfo = methodInfo;
            info.parmaArray = parmaArray;
        }
        #endregion
    }

    public override void OnStart()
    {
        timer = 0;
        isPlay = false;

        //roleBase.Animator.Play("None");
        if (resultClipInfo == null) return;

        #region 计算动画速度
        //动画速度缩放
        aniScale = 1;
        if (depend != AtkSpeedDepend.None)
        {
            //攻击前摇时长不依赖于动画播放，而是动画播放依赖于攻击前摇
            //当前攻速
            ValueTotal atkSpeedInfo = roleBase.GetRoleAttribute(RoleAttribute.AtkSpeed);
            float atkSpeed = atkSpeedInfo.Value;
            float scale = 1;

            switch (depend)
            {
                case AtkSpeedDepend.百分比:
                    {
                        //基础前摇
                        float baseBeforeAtk = dependPercent * atkSpeed;
                        //攻击前摇时间 = 基础攻击前摇 / (1 + 额外攻速)
                        float beforeAtk = baseBeforeAtk / (1 + atkSpeed - atkSpeedInfo.BaseValue);
                        //缩放比例 = 逻辑攻击奏效帧时间点 / 动画攻击奏效帧时间点
                        scale = beforeAtk / resultClipInfo.length;
                    }
                    break;
                case AtkSpeedDepend.固定值:
                    {
                        //缩放比例 = 逻辑攻击奏效帧时间点 / 动画攻击奏效帧时间点
                        scale = dependValue / resultClipInfo.length;
                    }
                    break;
                default:
                    break;
            }

            //动画速度缩放
            aniScale = 1 / scale;
            roleBase.Animator.SetFloat("atkSpeedAffect", aniScale);
            //事件点时间缩放
            for (int i = 0; i < aniEventInfos.Count; i++)
            {
                aniEventInfos[i].usetimePoint = aniEventInfos[i].timePoint * scale;
            }
        }

        aniMaxTime = resultClipInfo.length / aniScale;//动画播放时间
        //限制动画事件点位置
        for (int i = 0; i < aniEventInfos.Count; i++)
        {
            var item = aniEventInfos[i];
            item.usetimePoint = item.timePoint / aniScale;
            item.usetimePoint = Mathf.Clamp(item.usetimePoint, 0, aniMaxTime);
        }
        #endregion
    }

    private bool isPlay;
    public override TaskStatus OnUpdate()
    {
        if (resultClipInfo == null)
        {
            roleBase.Animator.Play("None");
            if (aniName != "None")
            {
                Debuger.ToError($"{gameObject.name}动画:{aniName}为空,不能调用");
            }
            return TaskStatus.Success;
        }

        #region 播放动画
        if (!isPlay)
        {
            isPlay = true;
            //是否已是目标片段
            if (roleBase.Animator.GetCurrentAnimatorStateInfo(0).IsName(resultClipInfo.name))
            {
                roleBase.Animator.Play(resultClipInfo.name);
            }
            else
            {
                //这里用动画长度和百分比的原因是放在动画倍数变慢影响融合
                roleBase.Animator.CrossFade(resultClipInfo.name, Mathf.Min(Utility.AniCrossTime, resultClipInfo.length));
            }
            #endregion
        }

        //立即退出
        if (ExitImmediately)
        {
            if (roleBase.Animator.GetCurrentAnimatorStateInfo(0).IsName(resultClipInfo.name))
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }

        //时间增加--动画状态机速度
        timer += Time.deltaTime * roleBase.Animator.speed;
        //事件触发
        for (int i = aniEventInfos.Count - 1; i >= 0; i--)
        {
            AniEventInfo item = aniEventInfos[i];
            if (timer >= item.usetimePoint && !item.isTrigger)
            {
                Invoke(item);
            }
        }

        if (timer >= aniMaxTime) return TaskStatus.Success;
        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        timer = 0;
        for (int i = 0; i < aniEventInfos.Count; i++)
        {
            AniEventInfo item = aniEventInfos[i];
            item.isTrigger = false;
        }
    }

    public override void OnReset()
    {
        for (int i = 0; i < aniEventInfos.Count; i++)
        {
            aniEventInfos[i].OnReset();
        }
    }

    /// <summary>
    /// 调用事件方法
    /// </summary>
    private void Invoke(AniEventInfo info)
    {
        info.InVoke();
        info.isTrigger = true;
    }
}

/// <summary>
/// 事件信息
/// </summary>
[Serializable]
public class AniEventInfo
{
    /// <summary>
    /// 触发时间点
    /// </summary>
    public float timePoint;
    [HideInInspector]
    /// <summary>
    /// 实际事件点
    /// </summary>
    public float usetimePoint;
    [HideInInspector]
    /// <summary>
    /// 是否触发
    /// </summary>
    public bool isTrigger;
    [HideInInspector]
    [Tooltip("目标物体")]
    public GameObject targetGameObject;
    [Tooltip("脚本名称")]
    public string componentName = "RoleBase";
    [Tooltip("方法名称")]
    public string methodName;
    [SerializeField]
    /// <summary>
    /// 参数列表
    /// </summary>
    public string parmeterString = "";

    //================Awake提前反射保存方法引用===================
    [HideInInspector]
    //脚本
    public Component component;
    [HideInInspector]
    //方法
    public System.Reflection.MethodInfo methodInfo;
    [HideInInspector]
    //参数
    public object[] parmaArray;

    //调用
    public void InVoke()
    {
        //反射调用
        methodInfo.Invoke(component, parmaArray);
    }

    public void OnReset()
    {
        componentName = null;
        methodName = null;
        parmeterString = "";
    }
}

public enum AtkSpeedDepend
{
    None,
    百分比,
    固定值,
}
