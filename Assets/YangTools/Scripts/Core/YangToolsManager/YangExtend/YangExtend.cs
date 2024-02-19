using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace YangTools.Extend
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static partial class YangExtend
    {
        //TODO /*读表 LuBan*/，时间，/*通用范型类*/，多语言，SDKManager,/*事件系统*/，/*定时器*/，广告，支付

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
            YangToolEventManager.Instance.Add(ret);
            return ret;
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="_listener">事件监听器实例</param>
        public static void RemoveEventListener(this UnityEngine.Object _object, EventInspector _listener)
        {
            YangToolEventManager.Instance.Remove(_listener);
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public static void RemoveEventListener(this UnityEngine.Object _object)
        {
            YangToolEventManager.Instance.Remove(_object);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="_object">物体</param>
        /// <param name="_eventName">事件名称</param>
        /// <param name="_args">事件参数</param>
        public static void SendEvent(this UnityEngine.Object _object, string _eventName, params object[] _args)
        {
            YangToolEventManager.Instance.Send(_eventName, _args);
        }
        #endregion
    }

    //——THE END——
}