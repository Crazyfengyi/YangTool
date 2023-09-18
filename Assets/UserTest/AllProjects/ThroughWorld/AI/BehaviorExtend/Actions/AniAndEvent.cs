///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-11 
//*/
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using UnityEngine;
//using System.Collections;
//using System;
//using System.Collections.Generic;
//using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
//using System.Reflection;

////[BehaviorDesigner.Runtime.Tasks.HelpURL("http://www.baidu.com")]
//[TaskIcon(BehaviorSetting.path + "AniAndEventIcon.png")]
//[TaskCategory("RoleAI/Animator")]
//[TaskDescription("注意:动画事件倒序遍历(同一时间点,后加先执行)")]
///// <summary>
///// 播放动画和触发事件
///// </summary>
//public class AniAndEvent : BehaviorDesigner.Runtime.Tasks.Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 目标动画
//    /// </summary>
//    public AnimationClip defaultAni;
//    /// <summary>
//    /// 备用动画
//    /// </summary>
//    public string reserveAniName;
//    /// <summary>
//    /// 是否融合
//    /// </summary>
//    public bool isBlend = true;
//    /// <summary>
//    /// 只调用播放
//    /// </summary>
//    public bool isOnlyPlay;
//    /// <summary>
//    /// 立即退出
//    /// </summary>
//    public bool ExitImmediately;
//    /// <summary>
//    /// 是否依赖固定速度
//    /// </summary>
//    public bool isDependSpeed;
//    /// <summary>
//    /// 攻速依赖值
//    /// </summary>
//    //public FloatUseEnum dependFloatUseEnum;
//    /// <summary>
//    /// 自定义动画时间
//    /// </summary>
//    public float customTime;
//    /// <summary>
//    /// 动画速度缩放
//    /// </summary>
//    private float aniScale;
//    /// <summary>
//    /// 事件信息
//    /// </summary>
//    public List<AniEventInfo> aniEventInfos = new List<AniEventInfo>();
//    /// <summary>
//    /// 有默认动画
//    /// </summary>
//    private bool haveDefaultAni;
//    /// <summary>
//    /// 时间
//    /// </summary>
//    private float timer;
//    /// <summary>
//    /// 动画片段长度(时间)
//    /// </summary>
//    private float aniMaxTime;
//    [HideInInspector]
//    //运行时拿到的动画
//    public AnimationClip resultClipInfo;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//        if (!roleBase.Animator)
//        {
//            Debuger.ToError($"{gameObject.name}的行为树中AniAndEvent找不到Animator");
//        }

//        #region 反射
//        for (int i = 0; i < aniEventInfos.Count; i++)
//        {
//            aniEventInfos[i].targetGameObject = gameObject;
//            AniEventInfo info = aniEventInfos[i];

//            //目标脚本(组件)
//            Type type = TaskUtility.GetTypeWithinAssembly(info.componentName);
//            if (type == null)
//            {
//                Debuger.ToError($"{gameObject.name}:无法调用-类型{info.componentName}为空");
//            }
//            //获得脚本
//            Component component = GetDefaultGameObject(info.targetGameObject).GetComponent(type);
//            if (component == null)
//            {
//                Debuger.ToError($"{gameObject.name}的脚本{info.componentName}为空,不能调用");
//            }

//            //方法
//            MethodInfo method = component.GetType().GetMethod(info.methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//            //方法参数
//            ParameterInfo[] parmaInfo = method.GetParameters();
//            //参数类型列表
//            List<Type> parameterTypeList = new List<Type>();
//            //参数数组
//            object[] parmaArray = new object[parmaInfo.Length];
//            //临时参数
//            string tempParma = string.Empty;
//            string[] parmeterList = info.parmeterString.Split('|');

//            for (int j = 0; j < parmaInfo.Length; j++)
//            {
//                if (j >= parmeterList.Length || string.IsNullOrEmpty(parmeterList[j]))
//                {
//                    if (parmaInfo[j].HasDefaultValue)
//                    {
//                        //用默认值
//                        parmaArray[j] = parmaInfo[j].DefaultValue;
//                        parameterTypeList.Add(parmaInfo[j].ParameterType);
//                        continue;
//                    }
//                    else
//                    {
//                        Debuger.ToError($"角色:{roleBase.transform.name}的动画{defaultAni?.name}-OR-{reserveAniName}的事件:{method}第{ j }个参数缺失,请填写所有参数!");
//                        throw null;
//                    }
//                }
//                else
//                {
//                    tempParma = parmeterList[j];
//                }

//                switch (Type.GetTypeCode(parmaInfo[j].ParameterType))
//                {
//                    case TypeCode.Int32:
//                        parmaArray[j] = int.Parse(tempParma);
//                        break;
//                    case TypeCode.Single:
//                        parmaArray[j] = float.Parse(tempParma);
//                        break;
//                    case TypeCode.Boolean:
//                        parmaArray[j] = bool.Parse(tempParma);
//                        break;
//                    case TypeCode.String:
//                        parmaArray[j] = tempParma;
//                        break;
//                    case TypeCode.Int64:
//                        parmaArray[j] = long.Parse(tempParma);
//                        break;
//                    default:
//                        Debuger.ToError("AniAndEvent中参数只支持int,float,string,bool");
//                        break;
//                }
//                parameterTypeList.Add(parmaInfo[j].ParameterType);
//            }

//            //获得方法
//            System.Reflection.MethodInfo methodInfo = component.GetType().GetMethod(info.methodName, parameterTypeList.ToArray());

//            if (methodInfo == null)
//            {
//                Debuger.ToError($"脚本{info.componentName}上方法{info.methodName}未找到,参数列表{parameterTypeList.ToArray()}");
//            }

//            info.component = component;
//            info.methodInfo = methodInfo;
//            info.parmaArray = parmaArray;
//        }
//        #endregion

//        #region 动画

//        try
//        {
//            if (defaultAni != null)
//            {
//                resultClipInfo = roleBase.Animator.GetAnimClipByName(defaultAni?.name);
//            }

//            //角色身上有这个动画片段
//            if (resultClipInfo == null)
//            {
//                resultClipInfo = roleBase.Animator.GetAnimClipByName(reserveAniName);
//            }
//        }
//        catch (Exception)
//        {
//            Debuger.ToError($"角色:{roleBase.transform.name}的{defaultAni?.name}-OR-{reserveAniName}动画出错");
//            throw;
//        }

//        #endregion
//    }
//    public override void OnStart()
//    {
//        if (resultClipInfo == null)
//        {
//            //Debuger.ToError("行为树动画clip为空:");
//            //Debuger.ToError($"角色:{roleBase.transform.name}的{defaultAni?.name}-OR-{reserveAniName}动画出错");
//        }

//        if (roleBase.Animator == null)
//        {
//            //Debuger.ToError("行为树动画Animator为空:");
//        }

//        timer = 0;

//        #region 播放动画
//        //是否只调用播放
//        if (isOnlyPlay)
//        {
//            if (isBlend)
//            {
//                //判断当前片段是否已经是目标片段
//                if (roleBase.Animator.GetCurrentAnimatorStateInfo(0).IsName(resultClipInfo.name))
//                {
//                    roleBase.Animator.Play(resultClipInfo.name);
//                }
//                else
//                {
//                    //这里用动画长度和百分比的原因是放在动画倍数变慢影响融合
//                    roleBase.Animator.CrossFade(resultClipInfo.name, Mathf.Min(ProjectSettingInfo.aniCrossTime, resultClipInfo.length));
//                }
//            }
//            else
//            {
//                roleBase.Animator.Play(resultClipInfo.name);
//            }
//        }
//        else
//        {
//            //是否需要融合
//            if (isBlend)
//            {
//                //判断当前片段是否已经是目标片段
//                if (roleBase.Animator.GetCurrentAnimatorStateInfo(0).IsName(resultClipInfo.name))
//                {
//                    roleBase.Animator.Play(resultClipInfo.name, 0, 0);
//                }
//                else
//                {
//                    try
//                    {
//                        //这里用动画长度和百分比的原因是放在动画倍数变慢影响融合
//                        roleBase.Animator.CrossFade(resultClipInfo.name, Mathf.Min(ProjectSettingInfo.aniCrossTime, resultClipInfo.length));
//                    }
//                    catch (Exception)
//                    {
//                        Debuger.ToError($"动画报错:角色为:{roleBase.transform.name},reserveAniName:{reserveAniName}");
//                        throw;
//                    }

//                }
//            }
//            else
//            {
//                roleBase.Animator.Play(resultClipInfo.name, 0, 0);
//            }
//        }

//        #endregion

//        #region 计算动画速度
//        //动画速度缩放
//        aniScale = 1;
//        if (isDependSpeed)
//        {
//            攻击前摇时长不依赖于动画播放，而是动画播放依赖于攻击前摇
//            攻速信息
//            ComputableValue atkSpeedInfo = roleBase.rolePropertyInfo.GetProperty(DataStruct.RolePropertyType.AttackSpeed);
//            最终攻速
//            float atkSpeed = atkSpeedInfo.CurrentValue;

//            float scale = 1;
//            switch (dependFloatUseEnum)
//            {
//                case FloatUseEnum.攻击基础前摇百分比:
//                    {
//                        基础前摇
//                        float baseBeforeAtk = roleBase.GetFloatMark(DataStruct.FloatUseEnum.攻击基础前摇百分比) * atkSpeed;
//                        攻击前摇时间 = 基础攻击前摇 / (1 + 额外攻速)
//                        float beforeAtk = baseBeforeAtk / (1 + atkSpeed - atkSpeedInfo.baseValue);
//                        缩放比例 = 逻辑攻击奏效帧时间点 / 动画攻击奏效帧时间点
//                        scale = beforeAtk / resultClipInfo.length;
//                    }
//                    break;
//                case FloatUseEnum.攻击基础后摇百分比:
//                    {
//                        基础后摇
//                        float baseAfterAtk = roleBase.GetFloatMark(DataStruct.FloatUseEnum.攻击基础后摇百分比) * atkSpeed;
//                        攻击后摇时间 = 基础攻击后摇 / (1 + 额外攻速)
//                        float afterAtk = baseAfterAtk / (1 + atkSpeed - atkSpeedInfo.baseValue);
//                        缩放比例 = 逻辑攻击奏效帧时间点 / 动画攻击奏效帧时间点
//                        scale = afterAtk / resultClipInfo.length;
//                    }
//                    break;
//                case FloatUseEnum.动画斧头特殊动作前固定时间:
//                case FloatUseEnum.动画斧头特殊动作中固定时间:
//                case FloatUseEnum.动画斧头特殊动作后固定时间:
//                    {
//                        float targetTime = roleBase.GetFloatMark(dependFloatUseEnum);
//                        缩放比例 = 逻辑攻击奏效帧时间点 / 动画攻击奏效帧时间点
//                        scale = targetTime / resultClipInfo.length;
//                    }
//                    break;
//                case FloatUseEnum.使用自定义动画时间:
//                    {
//                        缩放比例 = 逻辑攻击奏效帧时间点 / 动画攻击奏效帧时间点
//                        scale = customTime / resultClipInfo.length;
//                    }
//                    break;
//                default:
//                    break;
//            }

//            //动画速度缩放
//            aniScale = 1 / scale;

//            roleBase.Animator.SetFloat("atkSpeedAffect", aniScale);

//            //事件点时间缩放
//            for (int i = 0; i < aniEventInfos.Count; i++)
//            {
//                aniEventInfos[i].usetimePoint = aniEventInfos[i].timePoint * scale;
//            }
//        }
//        aniMaxTime = resultClipInfo.length / aniScale;//动画播放时间
//        //限制动画事件点位置
//        for (int i = 0; i < aniEventInfos.Count; i++)
//        {
//            var item = aniEventInfos[i];
//            item.usetimePoint = item.timePoint / aniScale;
//            item.usetimePoint = Mathf.Clamp(item.usetimePoint, 0, aniMaxTime);
//        }
//        #endregion
//    }

//    public override TaskStatus OnUpdate()
//    {
//        //立即退出
//        if (ExitImmediately)
//        {
//            if (roleBase.Animator.GetCurrentAnimatorStateInfo(0).IsName(resultClipInfo.name))
//            {
//                return TaskStatus.Success;
//            }
//            return TaskStatus.Running;
//        }

//        //时间增加---动画状态机速度
//        timer += Time.deltaTime * roleBase.Animator.speed;
//        //事件触发
//        for (int i = aniEventInfos.Count - 1; i >= 0; i--)
//        {
//            AniEventInfo item = aniEventInfos[i];
//            if (timer >= item.usetimePoint && !item.isTrigger)
//            {
//                Invoke(item);
//            }
//        }

//        if (timer >= aniMaxTime) return TaskStatus.Success;

//        return TaskStatus.Running;
//    }

//    public override void OnEnd()
//    {
//        timer = 0;
//        for (int i = 0; i < aniEventInfos.Count; i++)
//        {
//            AniEventInfo item = aniEventInfos[i];
//            item.isTrigger = false;
//        }
//    }

//    public override void OnReset()
//    {
//        for (int i = 0; i < aniEventInfos.Count; i++)
//        {
//            aniEventInfos[i].OnReset();
//        }
//    }

//    /// <summary>
//    /// 调用事件方法
//    /// </summary>
//    /// <param name="info">事件信息</param>
//    private void Invoke(AniEventInfo info)
//    {
//        info.InVoke();
//        info.isTrigger = true;
//    }
//}

///// <summary>
///// 事件信息
///// </summary>
//[Serializable]
//public class AniEventInfo
//{
//    /// <summary>
//    /// 触发时间点
//    /// </summary>
//    public float timePoint;
//    [HideInInspector]
//    /// <summary>
//    /// 实际事件点
//    /// </summary>
//    public float usetimePoint;
//    [HideInInspector]
//    /// <summary>
//    /// 是否触发
//    /// </summary>
//    public bool isTrigger;
//    [HideInInspector]
//    [Tooltip("目标物体")]
//    public GameObject targetGameObject;
//    [Tooltip("脚本名称")]
//    public string componentName = "RoleBase";
//    [Tooltip("方法名称")]
//    public string methodName;
//    [SerializeField]
//    /// <summary>
//    /// 参数列表
//    /// </summary>
//    public string parmeterString = "";

//    //================Awake提前反射保存方法引用===================
//    [HideInInspector]
//    //脚本
//    public Component component;
//    [HideInInspector]
//    //方法
//    public System.Reflection.MethodInfo methodInfo;
//    [HideInInspector]
//    //参数
//    public object[] parmaArray;

//    //调用
//    public void InVoke()
//    {
//        //反射调用
//        methodInfo.Invoke(component, parmaArray);
//    }

//    public void OnReset()
//    {
//        componentName = null;
//        methodName = null;
//        parmeterString = "";
//    }
//}
