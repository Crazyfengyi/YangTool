using YangTools.Scripts.Core.YangEvent;
using EventInfo = YangTools.Scripts.Core.YangEvent.EventInfo;

namespace YangTools.Scripts.Core.YangExtend
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
        /// <param name="eventCallback">事件回调</param>
        public static EventInfo AddEventListener(this UnityEngine.Object thisObject, string eventName,
            EventCallback<EventData> eventCallback)
        {
            var ret = new EventInfo(thisObject, eventName, eventCallback);
            YangEventManager.Instance.Add(ret);
            return ret;
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="thisObject">绑定的物体</param>
        /// <param name="listener">事件监听器实例</param>
        public static void RemoveEventListener(this UnityEngine.Object thisObject, EventInfo listener)
        {
            YangEventManager.Instance.Remove(listener); 
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public static void RemoveEventListener(this UnityEngine.Object thisObject)
        {
            YangEventManager.Instance.Remove(thisObject);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="thisObject">物体</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="args">事件参数</param>
        public static void SendEvent(this UnityEngine.Object thisObject, string eventName, IEventMessage args)
        {
            YangEventManager.Instance.Send(eventName, args);
        }

        #endregion
    }

    //——THE END——
}