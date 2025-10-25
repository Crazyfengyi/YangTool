using System;

namespace YangTools
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static partial class YangExtend
    {
        // TODO
        // 通用范型类--完成
        // 读表LuBan--完成
        // SDKManager--完成
        // 事件系统--完成
        // 定时器--完成
        // 广告--根据平台接SDK
        // 支付--版号问题,暂无
        // 存档-状态机-红点-资源管理YooAsset-对象池-交互管理器-小地图-引导管理器-声音管理器-UI界面管理器 --完成
        // 多语言(根据表来)--完成
        // 时间(初始获取时间,自增管理)--完成
        
        #region 事件系统扩展

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="thisObject">绑定的物体</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件回调</param>
        /// <param name="sortId"></param>
        public static EventInfo AddEventListener<T>(this UnityEngine.Object thisObject, Action<EventData> action,string eventName = "",int sortId = 0)
            where T : EventMessageBase
        {
            var targetName = typeof(T).FullName;
            if (!string.IsNullOrEmpty(eventName))
            {
                targetName = eventName;
            }
            
            var ret = new EventInfo(thisObject, targetName, action, sortId);
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