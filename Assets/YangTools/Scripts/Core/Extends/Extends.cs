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

        #region 事件系统扩展
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

    }

    //——THE END——
}