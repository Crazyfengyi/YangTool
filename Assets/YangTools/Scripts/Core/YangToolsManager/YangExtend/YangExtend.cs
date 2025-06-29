using System;

namespace YangTools
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
        /// <param name="thisObject">绑定的物体</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件回调</param>
        public static EventInfo AddEventListener<T>(this UnityEngine.Object thisObject, Action<EventData> action,string eventName = "")
            where T : EventMessageBase
        {
            var targetName = typeof(T).FullName;
            if (!string.IsNullOrEmpty(eventName))
            {
                targetName = eventName;
            }
            
            var ret = new EventInfo(thisObject, targetName, action);
            YangEventManager.Instance.Add(ret);
            return ret;
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="listener">事件监听器实例</param>
        public static void RemoveEventListener(EventInfo listener)
        {
            YangEventManager.Instance.Remove(listener); 
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public static void RemoveEventListener(UnityEngine.Object thisObject)
        {
            YangEventManager.Instance.Remove(thisObject);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="args">事件参数</param>
        public static void SendEvent(Type type, EventMessageBase args,string eventName = "") 
        {
            string targetName = type.FullName;
            if (!string.IsNullOrEmpty(eventName))
            {
                targetName = eventName;
            }
            YangEventManager.Instance.Send(targetName, args);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="args">事件参数</param>
        public static void SendEvent<T>(EventMessageBase args,string eventName = "") 
            where T : EventMessageBase 
        {
            string targetName = typeof(T).FullName;
            if (!string.IsNullOrEmpty(eventName))
            {
                targetName = eventName;
            }
            YangEventManager.Instance.Send(targetName, args);
        }
        #endregion
    }

    //——THE END——
}