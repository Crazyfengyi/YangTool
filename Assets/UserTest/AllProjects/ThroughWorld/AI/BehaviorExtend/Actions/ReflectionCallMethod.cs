///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-20 
//*/
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using UnityEngine;
//using System.Collections;
//using System;
//using YangToolDebuger;
//using System.Collections.Generic;
//using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
//using System.Reflection;

//[TaskIcon("{SkinColor}ReflectionIcon.png")]
//[TaskDescription("反射调用方法")]
//[TaskCategory("RoleAI/Reflection")]
//public class ReflectionCallMethod : BehaviorDesigner.Runtime.Tasks.Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 脚本名
//    /// </summary>
//    public string scriptName = "RoleBase";
//    /// <summary>
//    /// 方法名
//    /// </summary>
//    public string methodName;
//    /// <summary>
//    /// 参数列表
//    /// </summary>
//    public List<string> parmeterList = new List<string>();


//    public override void OnAwake()
//    {
//        base.OnAwake();
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {
//        base.OnStart();
//    }

//    public override TaskStatus OnUpdate()
//    {
//        Invoke();
//        return TaskStatus.Success;
//    }

//    /// <summary>
//    /// 调用事件方法
//    /// </summary>
//    /// <param name="info">事件信息</param>
//    public void Invoke()
//    {
//        //目标脚本(组件)
//        Type type = TaskUtility.GetTypeWithinAssembly(scriptName);
//        if (type == null)
//        {
//            Debuger.ToError($"{gameObject.name}:无法调用-类型{scriptName}为空");
//        }
//        //获得脚本
//        Component component = GetDefaultGameObject(roleBase.gameObject).GetComponent(type);
//        if (component == null)
//        {
//            Debuger.ToError($"{gameObject.name}的脚本{scriptName}为空,不能调用");
//        }
//        //参数类型列表
//        List<Type> parameterTypeList = new List<Type>();

//        //方法
//        MethodInfo method = component.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//        //方法参数
//        ParameterInfo[] parmaInfo = method.GetParameters();
//        //参数数组
//        object[] parmaArray = new object[parmaInfo.Length];
//        //临时参数
//        string tempParma = string.Empty;

//        for (int i = 0; i < parmaInfo.Length; i++)
//        {
//            if (i >= parmeterList.Count || string.IsNullOrEmpty(parmeterList[i]))
//            {
//                if (parmaInfo[i].HasDefaultValue)
//                {
//                    tempParma = parmaInfo[i].DefaultValue.ToString();//有没填的可选参数则取默认值
//                }
//                else
//                {
//                    Debuger.ToError($"方法:{method}第{ i }个参数缺失,请填写所有参数!");
//                    throw null;
//                }

//                parmaArray[i] = parmaInfo[i].DefaultValue;
//                parameterTypeList.Add(parmaInfo[i].ParameterType);
//                continue;
//            }

//            tempParma = parmeterList[i];

//            switch (Type.GetTypeCode(parmaInfo[i].ParameterType))
//            {
//                case TypeCode.Int32:
//                    parmaArray[i] = int.Parse(tempParma);
//                    break;
//                case TypeCode.Single:
//                    parmaArray[i] = float.Parse(tempParma);
//                    break;
//                case TypeCode.Boolean:
//                    parmaArray[i] = bool.Parse(tempParma);
//                    break;
//                case TypeCode.String:
//                    parmaArray[i] = tempParma;
//                    break;
//                case TypeCode.Int64:
//                    parmaArray[i] = long.Parse(tempParma);
//                    break;
//                default:
//                    Debuger.ToError("AniAndEvent中参数只支持int,float,string,bool");
//                    break;
//            }

//            parameterTypeList.Add(parmaInfo[i].ParameterType);
//        }

//        // If you are receiving a compiler error on the Windows Store platform see this topic:
//        // https://www.opsive.com/support/documentation/behavior-designer/installation/
//        //获得方法
//        System.Reflection.MethodInfo methodInfo = component.GetType().GetMethod(methodName, parameterTypeList.ToArray());

//        if (methodInfo == null)
//        {
//            Debuger.ToError($"脚本{scriptName}上方法{methodName}未找到,参数列表{parameterTypeList.ToString()}");
//        }

//        //反射调用
//        methodInfo.Invoke(component, parmaArray);
//    }
//}