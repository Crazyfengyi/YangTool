using System;
using System.Collections.Generic;
using UnityEngine;
using YangTools;

namespace YangTools
{
    /// <summary>
    /// 计时器管理者
    /// </summary>
    public class YangToolTimerManager : MonoBehaviour
    {
        #region 内部调用
        /// 单例
        /// </summary>
        private static YangToolTimerManager instance;
        /// <summary>
        /// 设备线程锁
        /// </summary>
        private static readonly object padlock = new object();
        /// <summary>
        /// 对外单例
        /// </summary>
        public static YangToolTimerManager Instacne
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        var timerObject = new GameObject("TimerManager");
                        instance = timerObject.AddComponent<YangToolTimerManager>();
                        timerObject.transform.SetParent(YangTools.DontDestoryObject.transform);
                    }

                    return instance;
                }
            }
        }
        /// <summary>
        /// 计时器列表
        /// </summary>
        private static readonly List<YangToolTimer> TimerList = new List<YangToolTimer>();
        /// <summary>
        /// 计时完成的计时器--暂存列表(需要删除)
        /// </summary>
        private static readonly List<YangToolTimer> AutoDestoryList = new List<YangToolTimer>();
        /// <summary>
        /// 主动移除的计时器--暂存列表(需要删除)
        /// </summary>
        private static readonly List<YangToolTimer> CallDestoryList = new List<YangToolTimer>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {

        }

        private void LateUpdate()
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

        /// <summary>
        /// 将计时器加入自动删除列表
        /// </summary>
        /// <param name="timer">计时器</param>
        private static void SetAutoDestory(YangToolTimer timer)
        {
            if (!AutoDestoryList.Contains(timer))
            {
                AutoDestoryList.Add(timer);
            }
        }

        #endregion

        #region 对外调用
        /// <summary>
        /// 计时器-帧
        /// </summary>
        /// <param name="holder">绑定在目标物体上(当物体被销毁时,计时器不会调用)，可以为null，表示一定会调</param>
        /// <param name="delayFrame">延时多少帧</param>
        /// <param name="callback">回调方法</param>
        /// <param name="loopCount">回调次数，-1表示循环调用</param>
        public static YangToolTimer AddFrameTimer(int delayFrame, System.Action callback, UnityEngine.Object holder = null, int loopCount = 1)
        {
            if (loopCount == -1)
            {
                loopCount = int.MinValue;
            }
            FrameTimerInfo info = new FrameTimerInfo(holder, delayFrame, callback, loopCount);
            YangToolTimer item = new YangToolTimer(info, SetAutoDestory);
            TimerList.Add(item);
            return item;
        }

        /// <summary>
        /// 计时器-秒
        /// </summary>
        /// <param name="holder">绑定在目标物体上(当物体被销毁时,计时器不会调用)，可以为null，表示一定会调</param>
        /// <param name="delaySecond">延时多少秒</param>
        /// <param name="callback">回调方法</param>
        /// <param name="_isScaled">是否受时间影响</param>
        /// <param name="loopCount">回调次数 -1：为无限循环</param>
        /// <param name="isJumppFirstFrame">第一帧是否跳过计算(不跳过的话延时0秒当前帧就会执行--建议用帧计时)</param>
        public static YangToolTimer AddSecondTimer(float delaySecond, System.Action callback, bool _isScaled = true, UnityEngine.Object holder = null, int loopCount = 1, bool isJumppFirstFrame = true)
        {
            if (loopCount == -1)
            {
                loopCount = int.MinValue;
            }
            SecondTimerInfo info = new SecondTimerInfo(holder, delaySecond, callback, loopCount, _isScaled, isJumppFirstFrame);
            YangToolTimer item = new YangToolTimer(info, SetAutoDestory);
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
        /// <param name="_isScaled">是否受时间影响</param>
        public static YangToolTimer AddSecondLoopTimer(System.Action callback, System.Func<bool> checkback = null, System.Action overback = null, float delaySecond = float.MinValue, bool _isScaled = true, UnityEngine.Object holder = null)
        {
            if (delaySecond == float.MinValue)
            {
                delaySecond = _isScaled ? Time.deltaTime : Time.unscaledDeltaTime;
            }

            SecondTimerInfo info = new SecondTimerInfo(holder, delaySecond, callback, checkback, overback, _isScaled);
            YangToolTimer item = new YangToolTimer(info, SetAutoDestory);
            TimerList.Add(item);
            return item;
        }

        /// <summary>
        /// 删除延时回调
        /// </summary>
        /// <param name="item">计时器</param>
        /// <returns>是否删除成功</returns>
        public static bool RemoveTimer(YangToolTimer item)
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