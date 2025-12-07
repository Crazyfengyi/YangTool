using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        /// <summary>
        /// 构造函数
        /// </summary>
        public YangTimer(TimerInfo argTimerInfo, Action<YangTimer> manager)
        {
            SetAutoDestroy = manager;
            TimerInfo = argTimerInfo;
        }
        /// <summary>
        /// 更新
        /// </summary>
        public void MyUpdate()
        {
            TimerInfo.Update();
            //更新完后检查是否需要销毁
            if (TimerInfo.NeedDestroy)
            {
                Destroy();
            }
        }
        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
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
            if (argObject != null)
            {
                WeakObject = new System.WeakReference<UnityEngine.Object>(argObject);
            }

            tag = _tag;
            Callback = argCallback;
            ExecuteCount = argExecuteCount;
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

        public FrameTimerInfo(UnityEngine.Object holder,string _tag, int argDelayFrame, Action argCallback, int count = 1, bool argIsJump = true)
            : base(holder,_tag, argCallback, count, false, argIsJump)
        {
            TargetFrame = argDelayFrame;
            CurrentFrame = TargetFrame;
        }
        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            //如果需要删除return
            if (NeedDestroy) return;

            //如果有绑定物体，但绑定物体已经没有存活
            if (base.WeakObject != null && !base.WeakObject.TryGetTarget(out UnityEngine.Object target))
            {
                base.NeedDestroy = true;
                return;
            }
            //第一帧跳过
            if (base.IsFirstUpdate)
            {
                base.IsFirstUpdate = false;
                return;
            }

            //时间流逝
            CurrentFrame--;
            //如果倒计时为0
            if (IsTimerComplete())
            {
                Callback?.Invoke();

                //如果是无限循环
                if (ExecuteCount == int.MinValue)
                {
                    CurrentFrame = TargetFrame;
                    return;
                }
                else
                {
                    ExecuteCount--;
                    if (ExecuteCount <= 0)
                    {
                        base.NeedDestroy = true;
                    }
                    else
                    {
                        CurrentFrame = TargetFrame;
                    }
                }
            }

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
            tag = _tag;
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
            //如果需要删除return
            if (NeedDestroy) return;

            //如果有绑定物体，但绑定物体已经没有存活
            if (base.WeakObject != null && !base.WeakObject.TryGetTarget(out UnityEngine.Object target))
            {
                base.NeedDestroy = true;
                return;
            }

            //第一帧跳过
            if (base.IsFirstUpdate)
            {
                base.IsFirstUpdate = false;
                return;
            }

            //是无限循环检查模式
            if (isInfiniteLoop)
            {
                //存在检查函数回调
                if (checkState != null)
                {
                    //检查是否需要结束
                    if (checkState.Invoke())
                    {
                        base.NeedDestroy = true;
                        overBack?.Invoke();
                        return;
                    }
                }
            }

            //时间流逝
            if (base.isScaled)
            {
                CurrentSecond -= Time.deltaTime;
            }
            else
            {
                CurrentSecond -= Time.unscaledDeltaTime;
            }

            //如果倒计时为0
            if (IsTimerComplete())
            {
                Callback?.Invoke();

                //如果是无限循环
                if (ExecuteCount == int.MinValue)
                {
                    CurrentSecond = DelaySecond;
                    return;
                }
                else
                {
                    ExecuteCount--;
                    if (ExecuteCount <= 0)
                    {
                        base.NeedDestroy = true;
                    }
                    else
                    {
                        CurrentSecond = DelaySecond;
                    }
                }
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