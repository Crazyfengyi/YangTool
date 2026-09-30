using System;
using UnityEngine;

namespace YangTools.Scripts.Core.YangTimer
{
    #region 计时器
    /// <summary>
    /// 计时器
    /// </summary>
    public class YangTimer
    {
        /// <summary>
        /// 计时信息
        /// </summary>
        public readonly TimerInfo TimerInfo;
        /// <summary>
        /// 设置自动删除--告诉管理类这个计时器需要删除
        /// </summary>
        public readonly Action<YangTimer> SetAutoDestroy;
        private bool destroyNotified; //是否已通知管理器销毁
        /// <summary>
        /// 构造函数
        /// </summary>
        public YangTimer(TimerInfo argTimerInfo, Action<YangTimer> manager)
        {
            SetAutoDestroy = manager;
            TimerInfo = argTimerInfo ?? throw new ArgumentNullException(nameof(argTimerInfo));
        }
        /// <summary>
        /// 更新
        /// </summary>
        public void MyUpdate()
        {
            MyUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// 使用管理器传入的时间推进并隔离业务回调异常
        /// </summary>
        public void MyUpdate(float deltaTime, float unscaledDeltaTime)
        {
            try
            {
                if (!TimerInfo.NeedDestroy)
                {
                    TimerInfo.Update(deltaTime, unscaledDeltaTime);
                }
            }
            catch (Exception exception)
            {
                TimerInfo.NeedDestroy = true;
                Debug.LogException(new InvalidOperationException($"计时器回调异常 [{TimerInfo.tag}]", exception));
            }
            finally
            {
                if (TimerInfo.NeedDestroy)
                {
                    Destroy();
                }
            }
        }
        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            TimerInfo.NeedDestroy = true;
            if (destroyNotified) return;

            destroyNotified = true;
            SetAutoDestroy?.Invoke(this);
        }
    }
    #endregion
    
    #region 计时信息
    /// <summary>
    /// 计时器信息
    /// </summary>
    public class TimerInfo
    {
        /// <summary>
        /// 标签
        /// </summary>
        public string tag;
        /// <summary>
        /// 是否需要销毁
        /// </summary>
        public bool NeedDestroy;
        /// <summary>
        /// 回调方法
        /// </summary>
        public readonly Action Callback;
        /// <summary>
        /// 执行次数
        /// </summary>
        public int ExecuteCount;
        /// <summary>
        /// 是否受Unity时间影响
        /// </summary>
        public readonly bool isScaled;
        /// <summary>
        /// 第一帧跳过
        /// </summary>
        public bool IsFirstUpdate;
        /// <summary>
        /// 弱引用-绑定物体
        /// </summary>
        public readonly System.WeakReference<UnityEngine.Object> WeakObject;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="argObject"></param>
        /// <param name="argCallback">回调方法</param>
        /// <param name="argExecuteCount">执行次数</param>
        /// <param name="argIsScaled">是否受Unity时间暂停影响</param>
        /// <param name="argIsJump">第一帧跳过</param>
        public TimerInfo(UnityEngine.Object argObject, string _tag,Action argCallback, int argExecuteCount, bool argIsScaled, bool argIsJump)
        {
            if (argExecuteCount < -1 && argExecuteCount != int.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(argExecuteCount));
            }

            //保留已销毁对象的绑定信息 避免将其误当成无绑定计时器
            if (!ReferenceEquals(argObject, null))
            {
                WeakObject = new System.WeakReference<UnityEngine.Object>(argObject);
            }

            tag = _tag;
            Callback = argCallback;
            ExecuteCount = argExecuteCount == -1 ? int.MinValue : argExecuteCount;
            NeedDestroy = ExecuteCount == 0;
            isScaled = argIsScaled;
            IsFirstUpdate = argIsJump;
        }

        /// <summary>
        /// 每帧刷新
        /// </summary>
        public virtual void Update()
        {
            Debug.LogError("TimerInfo is Update,This is error");
        }

        /// <summary>
        /// 接收更新时间并兼容已有无参更新实现
        /// </summary>
        public virtual void Update(float deltaTime, float unscaledDeltaTime)
        {
            Update();
        }

        /// <summary>
        /// 检查终止状态和绑定对象并处理首次更新跳过
        /// </summary>
        protected bool CanUpdate()
        {
            if (NeedDestroy) return false;
            if ((ExecuteCount <= 0 && ExecuteCount != int.MinValue) ||
                (WeakObject != null && (!WeakObject.TryGetTarget(out UnityEngine.Object target) || target == null)))
            {
                NeedDestroy = true;
                return false;
            }

            if (!IsFirstUpdate) return true;
            IsFirstUpdate = false;
            return false;
        }

        /// <summary>
        /// 调用回调并更新剩余次数 回调中的取消优先于后续循环
        /// </summary>
        protected void ExecuteCallback()
        {
            Callback?.Invoke();
            if (NeedDestroy || ExecuteCount == int.MinValue) return;

            ExecuteCount--;
            NeedDestroy = ExecuteCount <= 0;
        }
    };
    /// <summary>
    /// 帧计时器信息
    /// </summary>
    public class FrameTimerInfo : TimerInfo
    {
        /// <summary>
        /// 延迟多少帧
        /// </summary>
        public readonly int TargetFrame;
        /// <summary>
        /// 倒计时帧
        /// </summary>
        public int CurrentFrame;

        /// <summary>
        /// 创建帧计时器 零帧表示下次允许更新时执行
        /// </summary>
        public FrameTimerInfo(UnityEngine.Object holder,string _tag, int argDelayFrame, Action argCallback, int count = 1, bool argIsJump = true)
            : base(holder,_tag, argCallback, count, false, argIsJump)
        {
            if (argDelayFrame < 0) throw new ArgumentOutOfRangeException(nameof(argDelayFrame));
            TargetFrame = argDelayFrame;
            CurrentFrame = TargetFrame;
        }
        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            if (!CanUpdate()) return;
            CurrentFrame--;
            if (!IsTimerComplete()) return;
            ExecuteCallback();
            if (!NeedDestroy) CurrentFrame = TargetFrame;
        }
        /// <summary>
        /// 是否计时完成
        /// </summary>
        /// <returns></returns>
        public bool IsTimerComplete()
        {
            return CurrentFrame <= 0f;
        }
    }
    /// <summary>
    /// 秒计时器信息
    /// </summary>
    public class SecondTimerInfo : TimerInfo
    {
        /// <summary>
        /// 延时多少秒
        /// </summary>
        public readonly float DelaySecond;
        /// <summary>
        /// 倒计时(秒)
        /// </summary>
        public float CurrentSecond;
        //======下面是无限循环用的=======
        /// <summary>
        /// 是无限循环加条件检查模式
        /// </summary>
        private readonly bool isInfiniteLoop;
        /// <summary>
        /// 检查状态(检查是否需要结束)
        /// </summary>
        private readonly Func<bool> checkState;
        /// <summary>
        /// 结束回调
        /// </summary>
        private readonly Action overBack;
        //=======THE END===============
        /// <summary>
        /// 普通计时器
        /// </summary>
        public SecondTimerInfo(UnityEngine.Object holder,string _tag, float argDelayTime, Action argCallback, int count = 1, bool argIsScaled = true, bool argIsJump = true)
            : base(holder, _tag,argCallback, count, argIsScaled, argIsJump)
        {
            ValidateDelay(argDelayTime);
            DelaySecond = argDelayTime;
            CurrentSecond = DelaySecond;
            isInfiniteLoop = false;
        }
        /// <summary>
        /// 无限循环用
        /// </summary>
        public SecondTimerInfo(UnityEngine.Object holder,string _tag, float argDelayTime, Action argCallback, Func<bool> argCheckState, Action argOverBack, bool argIsScaled = true, bool argIsJump = true)
         : base(holder,_tag, argCallback, int.MinValue, argIsScaled, argIsJump)
        {
            ValidateDelay(argDelayTime);
            DelaySecond = argDelayTime;
            CurrentSecond = DelaySecond;

            isInfiniteLoop = true;
            checkState = argCheckState;
            overBack = argOverBack;
        }
        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// 推进秒计时 每次更新最多触发一次并保留周期内的时间余量
        /// </summary>
        public override void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (!CanUpdate()) return;
            if (isInfiniteLoop && checkState != null)
            {
                bool isOver = checkState.Invoke();
                if (NeedDestroy) return;
                if (isOver)
                {
                    NeedDestroy = true;
                    overBack?.Invoke();
                    return;
                }
            }

            float elapsed = isScaled ? deltaTime : unscaledDeltaTime; //本次流逝时间
            if (elapsed <= 0f || float.IsNaN(elapsed) || float.IsInfinity(elapsed)) return;
            CurrentSecond -= elapsed;
            if (!IsTimerComplete()) return;

            ExecuteCallback();
            if (NeedDestroy) return;
            //跨越多个周期时舍弃补发次数 保留下一次触发的相位
            CurrentSecond = DelaySecond > 0f ? DelaySecond + CurrentSecond % DelaySecond : 0f;
        }

        /// <summary>
        /// 校验秒间隔 防止无效数值产生永久挂起的计时器
        /// </summary>
        private static void ValidateDelay(float delay)
        {
            if (delay < 0f || float.IsNaN(delay) || float.IsInfinity(delay))
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }
        }
        /// <summary>
        /// 是否计时完成
        /// </summary>
        /// <returns></returns>
        public bool IsTimerComplete()
        {
            return CurrentSecond <= 0f;
        }
    }
    #endregion
}
