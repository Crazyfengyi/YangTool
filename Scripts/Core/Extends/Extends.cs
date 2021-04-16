using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static partial class Extends
    {
        //TODO 读表，时间，通用范型类，多语言，SDKManager,/*事件系统*/，/*定时器*/，广告，支付

        #region 事件监听器扩展
        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="_object">绑定物体</param>
        /// <param name="_eventName">事件名称</param>
        /// <param name="_eventCallback">事件回调</param>
        public static EventInspector AddEventListener(this UnityEngine.Object _object, string _eventName, EventCallback<EventInfo> _eventCallback)
        {
            var ret = new EventInspector(_object, _eventName, _eventCallback);
            YangToolEventListener.Instance.Add(ret);
            return ret;
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="_listener">事件监听器实例</param>
        public static void RemoveEventListener(this UnityEngine.Object _object, EventInspector _listener)
        {
            YangToolEventListener.Instance.Remove(_listener);
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public static void RemoveEventListener(this UnityEngine.Object _object)
        {
            YangToolEventListener.Instance.Remove(_object);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="_object">物体</param>
        /// <param name="_eventName">事件名称</param>
        /// <param name="_args">事件参数</param>
        public static void SendEvent(this UnityEngine.Object _object, string _eventName, params object[] _args)
        {
            YangToolEventListener.Instance.Send(_eventName, _args);
        }
        #endregion

        #region 小工具扩展

        /// <summary>
        /// 检查字段是否包含某个特性
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        public static bool HasAttribute<T>(this FieldInfo fieldInfo)
            where T : System.Attribute
        {
            try
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(T), false).Cast<T>();
                return attrs.First() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从一组权重信息中根据权重获得一个信息
        /// </summary>
        /// <typeparam name="T">权重信息类</typeparam>
        /// <param name="weightInfos">权重信息组</param>
        /// <param name="deleteFromInfosWhenFind">返回结果后，是否将该结果从组里删除（只有支持删除操作的容器才可以使用该功能）</param>
        public static T GetRandomInfo<T>(this IEnumerable<T> weightInfos, bool deleteFromInfosWhenFind = false)
            where T : WeightInfo
        {
            return WeightInfo.GetRandomInfo(weightInfos, deleteFromInfosWhenFind);
        }

        #endregion
    }

    //——THE END——
}

