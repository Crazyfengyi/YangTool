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
        /// 计时完成的计时器--暂存列表(需要删除)
        /// </summary>
        private static readonly List<YangTimer> AutoDestoryList = new List<YangTimer>();

        /// <summary>
        /// 主动移除的计时器--暂存列表(需要删除)
        /// </summary>
        private static readonly List<YangTimer> CallDestoryList = new List<YangTimer>();

        #endregion

        #region 生命周期

        public YangTimerManager()
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal override void InitModule()
        {
        }

        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            //移除主动删除的计时器
            for (int i = CallDestoryList.Count - 1; i >= 0; i--)
            {
                if (TimerList.Contains(CallDestoryList[i]))
                {
                    TimerList.Remove(CallDestoryList[i]);
                }
            }

            CallDestoryList.Clear();

            //调用计时器的update--正序遍历,保证调用顺序为添加先后
            for (int i = 0; i < TimerList.Count; i++)
            {
                TimerList[i].MyUpdate();
            }

            //移除计时完成的计时器
            for (int i = AutoDestoryList.Count - 1; i >= 0; i--)
            {
                if (TimerList.Contains(AutoDestoryList[i]))
                {
                    TimerList.Remove(AutoDestoryList[i]);
                }
            }

            AutoDestoryList.Clear();
        }

        internal override void CloseModule()
        {
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 将计时器加入自动删除列表
        /// </summary>
        /// <param name="timer">计时器</param>
        private static void SetAutoDestroy(YangTimer timer)
        {
            if (!AutoDestoryList.Contains(timer))
            {
                AutoDestoryList.Add(timer);
            }
        }

        #endregion

        #endregion

        #region 对外调用

        /// <summary>
        /// 计时器-帧
        /// </summary>
        /// <param name="holder">绑定在目标物体上(当物体被销毁时,计时器不会调用)，可以为null，表示一定会调</param>
        /// <param name="delayFrame">延时多少帧</param>
        /// <param name="callback">回调方法</param>
        /// <param name="loopCount">回调次数,-1表示循环调用</param>
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
            YangTimer item = new YangTimer(info, SetAutoDestroy);
            TimerList.Add(item);
            return item;
        }

        /// <summary>
        /// 计时器-秒
        /// </summary>
        /// <param name="holder">绑定在目标物体上(当物体被销毁时,计时器不会调用)，可以为null，表示一定会调</param>
        /// <param name="delaySecond">延时多少秒</param>
        /// <param name="callback">回调方法</param>
        /// <param name="isScaled">是否受时间影响</param>
        /// <param name="loopCount">回调次数 -1：为无限循环</param>
        /// <param name="isJumpFirstFrame">第一帧是否跳过计算(不跳过的话延时0秒当前帧就会执行--建议用帧计时)</param>
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
            YangTimer item = new YangTimer(info, SetAutoDestroy);
            TimerList.Add(item);
            return item;
        }

        /// <summary>
        /// 计时器-秒,无限循环,默认Time.deltaTime间隔
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
            if (Mathf.Approximately(delaySecond, float.MinValue))
            {
                delaySecond = isScaled ? Time.deltaTime : Time.unscaledDeltaTime;
            }

            if (string.IsNullOrEmpty(tag))
            {
                tag = autoTag;
            }

            SecondTimerInfo info = new SecondTimerInfo(holder, tag, delaySecond, callback, checkback, overback, isScaled);
            YangTimer item = new YangTimer(info, SetAutoDestroy);
            TimerList.Add(item);
            return item;
        }

        /// <summary>
        /// 删除延时回调
        /// </summary>
        /// <param name="item">计时器</param>
        /// <returns>是否删除成功</returns>
        public static bool RemoveTimer(YangTimer item)
        {
            if (TimerList.Contains(item))
            {
                CallDestoryList.Add(item);
                return true;
            }

            return false;
        }

        #endregion
    }
}