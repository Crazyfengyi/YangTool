using UnityEngine;

/// <summary>签到奖励旋转提示动画</summary>
public class DoRotateAniScript : MonoBehaviour
{
    public bool autoPlay;
    public float rotate = 10f;
    private bool playing;
    private Vector3 startRotate;

    /// <summary>记录初始角度并设置自动播放</summary>
    private void Awake()
    {
        startRotate = transform.localEulerAngles;
        playing = autoPlay;
    }

    /// <summary>更新旋转提示动画</summary>
    private void Update()
    {
        if (playing)
        {
            transform.localEulerAngles = startRotate + Vector3.forward * (Mathf.Sin(Time.unscaledTime * 6f) * rotate);
        }
    }

    /// <summary>开始动画</summary>
    public void StartAni() => playing = true;

    /// <summary>结束动画</summary>
    public void EndAni()
    {
        playing = false;
        transform.localEulerAngles = startRotate;
    }
}
