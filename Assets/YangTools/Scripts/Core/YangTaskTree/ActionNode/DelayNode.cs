using UnityEngine;
using YangTools.Scripts.Core.YangTimer;

public class DelayNode : ActionNodeBase
{
    private YangTimer timer;

    public float delayTime;

    public DelayNode(float time)
    {
        delayTime = time;
    }

    protected override void OnStart()
    {
        StartTimer();
    }

    private void StartTimer()
    {
        StopTimer();
        Debug.LogError($"等待{delayTime}秒");
        timer = YangTimerManager.AddSecondTimer(delayTime, OnTimerComplete, tag: "任务树延迟节点");
    }

    private void OnTimerComplete()
    {
        SetFinish(true);
    }

    private void StopTimer()
    {
        if (timer != null)
        {
            YangTimerManager.RemoveTimer(timer);
            timer = null;
        }
    }

    protected override void OnCancel()
    {
        StopTimer();
    }
}