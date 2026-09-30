using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using YangTools.Scripts.Core;

namespace YangTools.Scripts.Core.YangTimer
{
    /// <summary>
    /// 计时器管理者
    /// </summary>
    public class YangTimerManager : GameModuleBase
    {
        #region 内部调用

        #region 属性

        /// <summary>
        /// 计时器列表
        /// </summary>
        private static readonly List<YangTimer> TimerList = new List<YangTimer>();

        /// <summary>
        /// 等待下一轮加入的计时器
        /// </summary>
        private static readonly List<YangTimer> PendingTimers = new List<YangTimer>();

        /// <summary>
        /// 已注册计时器 用于快速判断归属
        /// </summary>
        private static readonly HashSet<YangTimer> RegisteredTimers = new HashSet<YangTimer>();
        private static bool isUpdating; //是否正在更新
        private static bool isClosed; //模块是否已关闭

        #endregion

        #region 生命周期

        /// <summary>
        /// 创建计时器管理模块
        /// </summary>
        public YangTimerManager()
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal override void InitModule()
        {
            isClosed = false;
        }

        /// <summary>
        /// 按注册顺序推进计时器并统一清理终止项
        /// </summary>
        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            if (isUpdating || isClosed) return;
            TimerList.AddRange(PendingTimers);
            PendingTimers.Clear();
            isUpdating = true;
            try
            {
                for (int i = 0; i < TimerList.Count; i++)
                {
                    TimerList[i].MyUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
                }
            }
            finally
            {
                isUpdating = false;
                RemoveDestroyedTimers(TimerList);
                RemoveDestroyedTimers(PendingTimers);
            }
        }

        /// <summary>
        /// 关闭模块并释放所有待执行的回调引用
        /// </summary>
        internal override void CloseModule()
        {
            isClosed = true;
            foreach (YangTimer timer in RegisteredTimers)
            {
                timer.TimerInfo.NeedDestroy = true;
            }

            TimerList.Clear();
            PendingTimers.Clear();
            RegisteredTimers.Clear();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 标记终止 由更新结束后的清理统一移除
        /// </summary>
        /// <param name="timer">计时器</param>
        private static void SetAutoDestroy(YangTimer timer)
        {
            timer.TimerInfo.NeedDestroy = true;
        }

        /// <summary>
        /// 注册计时器 新增项始终从下一轮更新开始处理
        /// </summary>
        private static YangTimer RegisterTimer(TimerInfo info)
        {
            if (isClosed) throw new System.InvalidOperationException("计时器模块已关闭");
            YangTimer timer = new YangTimer(info, SetAutoDestroy); //计时器句柄
            if (!info.NeedDestroy)
            {
                RegisteredTimers.Add(timer);
                PendingTimers.Add(timer);
            }

            return timer;
        }

        /// <summary>
        /// 单次压缩移除终止项并保持剩余计时器的注册顺序
        /// </summary>
        private static void RemoveDestroyedTimers(List<YangTimer> timers)
        {
            int remainingCount = 0; //保留数量
            for (int i = 0; i < timers.Count; i++)
            {
                YangTimer timer = timers[i]; //当前计时器
                if (timer.TimerInfo.NeedDestroy)
                {
                    RegisteredTimers.Remove(timer);
                    continue;
                }

                timers[remainingCount++] = timer;
            }

            if (remainingCount < timers.Count)
            {
                timers.RemoveRange(remainingCount, timers.Count - remainingCount);
            }
        }

        #endregion

        #endregion

        #region 对外调用

        /// <summary>
        /// 计时器-帧
        /// </summary>
        /// <remarks>保持跳过首次更新的行为 延迟一帧会在第二次管理器更新时执行</remarks>
        /// <param name="holder">绑定在目标物体上(当物体被销毁时,计时器不会调用)，可以为null，表示一定会调</param>
        /// <param name="delayFrame">延时多少帧</param>
        /// <param name="callback">回调方法</param>
        /// <param name="loopCount">回调次数 零次不执行 负一表示无限循环</param>
        /// <param name="tag">标签</param>
        /// <param name="autoTag">自动获取调用方法名称</param>
        public static YangTimer AddFrameTimer(int delayFrame, System.Action callback, UnityEngine.Object holder = null,
            int loopCount = 1, string tag = "", [CallerMemberName] string autoTag = "")
        {
            if (loopCount == -1)
            {
                loopCount = int.MinValue;
            }

            if (string.IsNullOrEmpty(tag))
            {
                tag = autoTag;
            }

            FrameTimerInfo info = new FrameTimerInfo(holder, tag, delayFrame, callback, loopCount);
            return RegisterTimer(info);
        }

        /// <summary>
        /// 计时器-秒
        /// </summary>
        /// <param name="holder">绑定在目标物体上(当物体被销毁时,计时器不会调用)，可以为null，表示一定会调</param>
        /// <param name="delaySecond">延时多少秒</param>
        /// <param name="callback">回调方法</param>
        /// <param name="isScaled">是否受时间影响</param>
        /// <param name="loopCount">回调次数 零次不执行 负一表示无限循环</param>
        /// <param name="isJumpFirstFrame">是否跳过首次管理器更新 回调中新建的计时器最早从下一轮处理</param>
        /// <param name="tag">标签</param>
        /// <param name="autoTag">自动获取调用方法名称</param>
        public static YangTimer AddSecondTimer(float delaySecond, System.Action callback,
            bool isScaled = true, UnityEngine.Object holder = null, int loopCount = 1, bool isJumpFirstFrame = true,
            string tag = "", [CallerMemberName] string autoTag = "")
        {
            if (loopCount == -1)
            {
                loopCount = int.MinValue;
            }

            if (string.IsNullOrEmpty(tag))
            {
                tag = autoTag;
            }

            SecondTimerInfo info = new SecondTimerInfo(holder, tag, delaySecond, callback, loopCount, isScaled,
                isJumpFirstFrame);
            return RegisterTimer(info);
        }

        /// <summary>
        /// 无限循环秒计时器 默认在时间推进时每轮执行一次
        /// </summary>
        /// <param name="holder">绑定在目标物体上(当物体被销毁时,计时器不会调用),可以为null,表示一定会调</param>
        /// <param name="delaySecond">间隔多少秒</param>
        /// <param name="callback">每个间隔回调函数</param>
        /// <param name="checkback">检查函数</param>
        /// <param name="overback">结束时调用函数</param>
        /// <param name="isScaled">是否受时间影响</param>
        /// <param name="tag">标签</param>
        /// <param name="autoTag">自动获取调用方法名称</param>
        public static YangTimer AddSecondLoopTimer(System.Action callback,
            System.Func<bool> checkback = null, System.Action overback = null, float delaySecond = float.MinValue,
            bool isScaled = true, UnityEngine.Object holder = null, string tag = "",
            [CallerMemberName] string autoTag = "")
        {
            if (delaySecond == float.MinValue)
            {
                delaySecond = 0f;
            }

            if (string.IsNullOrEmpty(tag))
            {
                tag = autoTag;
            }

            SecondTimerInfo info = new SecondTimerInfo(holder, tag, delaySecond, callback, checkback, overback, isScaled);
            return RegisterTimer(info);
        }

        /// <summary>
        /// 立即终止延时回调 重复取消或未注册时返回失败
        /// </summary>
        /// <param name="item">计时器</param>
        /// <returns>是否删除成功</returns>
        public static bool RemoveTimer(YangTimer item)
        {
            if (item != null && RegisteredTimers.Contains(item) && !item.TimerInfo.NeedDestroy)
            {
                item.Destroy();
                return true;
            }

            return false;
        }

        #endregion
    }
}
