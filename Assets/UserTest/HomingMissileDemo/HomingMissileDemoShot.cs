using System;
using UnityEngine;

namespace YangTools.Demos
{
    /// <summary>
    /// 播放单枚演示弹体的分段飞行动画并等待烟迹消散
    /// </summary>
    public sealed class HomingMissileDemoShot : MonoBehaviour
    {
        [SerializeField] private GameObject body; // 弹体与尾焰
        [SerializeField] private ParticleSystem smoke; // 世界空间烟迹

        private Transform target; // 演示靶标
        private Action<Vector3> onArrival; // 到达靶标时的特效回调
        private Vector3 origin; // 发射位置
        private Vector3 side; // 用于生成横向偏移的固定轴
        private float spread; // 扇形中的横向展开距离
        private float seed; // 每枚弹体独立的噪声采样起点
        private float spreadTime; // 散开时长
        private float wanderTime; // 摆动时长
        private float gatherTime; // 收束时长
        private float wanderStrength; // 摆动幅度
        private float elapsed; // 已播放时间
        private bool flying; // 飞行动画是否仍在播放

        /// <summary>
        /// 初始化路径并启动预制体上的烟雾
        /// </summary>
        internal void Launch(
            Transform destination, float fanOffset, float noiseSeed, Vector3 phaseTimes,
            float strength, Action<Vector3> arrival)
        {
            target = destination;
            onArrival = arrival;
            origin = transform.position;
            side = Vector3.Cross(Vector3.up, transform.forward).normalized;
            spread = fanOffset;
            seed = noiseSeed;
            spreadTime = phaseTimes.x;
            wanderTime = phaseTimes.y;
            gatherTime = phaseTimes.z;
            wanderStrength = strength;
            flying = true;
            smoke.Play();
        }

        /// <summary>
        /// 更新朝向和位置并在结束后清理烟迹
        /// </summary>
        private void Update()
        {
            if (!flying)
            {
                if (!smoke.IsAlive())
                    Destroy(gameObject);
                return;
            }

            if (target == null)
            {
                Finish(false);
                return;
            }

            float duration = spreadTime + wanderTime + gatherTime;
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
            Vector3 next = EvaluatePath(target.position, elapsed, duration);
            Vector3 tangent = next - transform.position;
            if (tangent.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.LookRotation(tangent);

            transform.position = next;
            if (elapsed >= duration)
                Finish(true);
        }

        /// <summary>
        /// 计算扇形展开 平滑摆动和靶标处归零的空间偏移
        /// </summary>
        private Vector3 EvaluatePath(Vector3 destination, float time, float duration)
        {
            float progress = time / duration;
            float opening = Mathf.SmoothStep(0f, 1f, time / spreadTime);
            float gathering = 1f - Mathf.SmoothStep(0f, 1f, (time - spreadTime - wanderTime) / gatherTime);
            float noiseTime = time * 1.85f;
            float sideways = (Mathf.PerlinNoise(seed, noiseTime) - 0.5f) * 2f;
            float vertical = (Mathf.PerlinNoise(seed + 31f, noiseTime) - 0.5f) * 2f;
            float arc = 4f * progress * (1f - progress) * (6f + Mathf.Abs(spread) * 0.45f);

            return Vector3.Lerp(origin, destination, progress)
                + side * (opening * gathering * (spread + sideways * wanderStrength))
                + Vector3.up * (arc + opening * gathering * vertical * wanderStrength * 0.7f);
        }

        /// <summary>
        /// 隐藏弹体并停止发射新烟雾 让已有粒子自然退场
        /// </summary>
        private void Finish(bool arrived)
        {
            flying = false;
            body.SetActive(false);
            smoke.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            if (arrived)
                onArrival?.Invoke(transform.position);
            onArrival = null;
        }
    }
}
