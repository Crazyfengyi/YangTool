using System;
using UnityEngine;

namespace YangTools.Timer
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
        public readonly TimerInfo timerInfo;
        /// <summary>
        /// 设置自动删除--告诉管理类这个计时器需要删除
        /// </summary>
        public Action<YangTimer> setAutoDestory;
        /// <summary>
        /// 构造函数
        /// </summary>
        public YangTimer(TimerInfo _timerInfo, Action<YangTimer> manager)
        {
            setAutoDestory = manager;
            timerInfo = _timerInfo;
        }
        /// <summary>
        /// 更新
        /// </summary>
        public void MyUpdate()
        {
            timerInfo.Update();
            //更新完后检查是否需要销毁
            if (timerInfo.needDeatory)
            {
                Destroy();
            }
        }
        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            setAutoDestory?.Invoke(this);
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
        /// 是否需要销毁
        /// </summary>
        public bool needDeatory = false;
        /// <summary>
        /// 回调方法
        /// </summary>
        public Action callback;
        /// <summary>
        /// 执行次数
        /// </summary>
        public int executeCount;
        /// <summary>
        /// 是否受Unity时间影响
        /// </summary>
        public bool isScaled;
        /// <summary>
        /// 第一帧跳过
        /// </summary>
        public bool isFirstUpdate = true;
        /// <summary>
        /// 弱引用-绑定物体
        /// </summary>
        public System.WeakReference<UnityEngine.Object> weakObject;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_callback">回调方法</param>
        /// <param name="_executeCount">执行次数</param>
        /// <param name="_isScaled">是否受Unity时间暂停影响</param>
        /// <param name="_isJump">第一帧跳过</param>
        public TimerInfo(UnityEngine.Object _object, Action _callback, int _executeCount, bool _isScaled, bool _isJump)
        {
            if (_object != null)
            {
                weakObject = new System.WeakReference<UnityEngine.Object>(_object);
            }
            callback = _callback;
            executeCount = _executeCount;
            isScaled = _isScaled;
            isFirstUpdate = _isJump;
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
        public int targetFrame;
        /// <summary>
        /// 倒计时帧
        /// </summary>
        public int currentFrame;

        public FrameTimerInfo(UnityEngine.Object holder, int _delayFrame, Action _callback, int count = 1, bool _isJump = true)
            : base(holder, _callback, count, false, _isJump)
        {
            targetFrame = _delayFrame;
            currentFrame = targetFrame;
        }
        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            //如果需要删除return
            if (needDeatory) return;

            //如果有绑定物体，但绑定物体已经没有存活
            if (base.weakObject != null && !base.weakObject.TryGetTarget(out UnityEngine.Object target))
            {
                base.needDeatory = true;
                return;
            }
            //第一帧跳过
            if (base.isFirstUpdate)
            {
                base.isFirstUpdate = false;
                return;
            }

            //时间流逝
            currentFrame--;
            //如果倒计时为0
            if (IsTimerComplete())
            {
                callback?.Invoke();

                //如果是无限循环
                if (executeCount == int.MinValue)
                {
                    currentFrame = targetFrame;
                    return;
                }
                else
                {
                    executeCount--;
                    if (executeCount <= 0)
                    {
                        base.needDeatory = true;
                    }
                    else
                    {
                        currentFrame = targetFrame;
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
            return currentFrame <= 0f;
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
        public float delaySecond;
        /// <summary>
        /// 倒计时(秒)
        /// </summary>
        public float currentSecond;
        //======下面是无限循环用的=======
        /// <summary>
        /// 是无限循环加条件检查模式
        /// </summary>
        private bool isInfiniteLoop;
        /// <summary>
        /// 检查状态(检查是否需要结束)
        /// </summary>
        private Func<bool> checkState;
        /// <summary>
        /// 结束回调
        /// </summary>
        private Action overBack;
        //=======THE END===============
        /// <summary>
        /// 普通计时器
        /// </summary>
        public SecondTimerInfo(UnityEngine.Object holder, float _delayTime, Action _callback, int count = 1, bool _isScaled = true, bool _isJump = true)
            : base(holder, _callback, count, _isScaled, _isJump)
        {
            delaySecond = _delayTime;
            currentSecond = delaySecond;
            isInfiniteLoop = false;
        }
        /// <summary>
        /// 无限循环用
        /// </summary>
        public SecondTimerInfo(UnityEngine.Object holder, float _delayTime, Action _callback, Func<bool> _checkState, Action _overBack, bool _isScaled = true, bool _isJump = true)
         : base(holder, _callback, int.MinValue, _isScaled, _isJump)
        {
            delaySecond = _delayTime;
            currentSecond = delaySecond;

            isInfiniteLoop = true;
            checkState = _checkState;
            overBack = _overBack;
        }
        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            //如果需要删除return
            if (needDeatory) return;

            //如果有绑定物体，但绑定物体已经没有存活
            if (base.weakObject != null && !base.weakObject.TryGetTarget(out UnityEngine.Object target))
            {
                base.needDeatory = true;
                return;
            }

            //第一帧跳过
            if (base.isFirstUpdate)
            {
                base.isFirstUpdate = false;
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
                        base.needDeatory = true;
                        overBack?.Invoke();
                        return;
                    }
                }
            }

            //时间流逝
            if (base.isScaled)
            {
                currentSecond -= Time.deltaTime;
            }
            else
            {
                currentSecond -= Time.unscaledDeltaTime;
            }

            //如果倒计时为0
            if (IsTimerComplete())
            {
                callback?.Invoke();

                //如果是无限循环
                if (executeCount == int.MinValue)
                {
                    currentSecond = delaySecond;
                    return;
                }
                else
                {
                    executeCount--;
                    if (executeCount <= 0)
                    {
                        base.needDeatory = true;
                    }
                    else
                    {
                        currentSecond = delaySecond;
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
            return currentSecond <= 0f;
        }
    }
    #endregion
}