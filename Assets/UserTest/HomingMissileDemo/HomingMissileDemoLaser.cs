using UnityEngine;

namespace YangTools.Demos
{
    /// <summary>
    /// 显示持续光束及其落点火花
    /// </summary>
    public sealed class HomingMissileDemoLaser : MonoBehaviour
    {
        private const int SegmentCount = 28; // 光束分段数
        private const float FullTurn = 2f * Mathf.PI; // 一个完整周期的弧度

        [SerializeField] private LineRenderer core; // 青白光芯
        [SerializeField] private LineRenderer glow; // 半透明外晕
        [SerializeField] private ParticleSystem sparks; // 靶标端火花
        [SerializeField, Min(0f)] private float waveAmplitude = 0.4f; // 最大横向摆幅
        [SerializeField, Min(0f)] private float waveFrequency = 2f; // 主波每秒摆动次数

        private readonly Vector3[] beamPoints = new Vector3[SegmentCount + 1]; // 两层渲染器共用的采样点

        /// <summary>
        /// 将光束和火花同步到当前发射点与靶心
        /// </summary>
        internal void DrawBeam(Vector3 origin, Vector3 destination, bool curved)
        {
            int pointCount = curved ? beamPoints.Length : 2;
            if (core.positionCount != pointCount || glow.positionCount != pointCount)
            {
                core.positionCount = pointCount;
                glow.positionCount = pointCount;
            }

            if (curved)
            {
                UpdateBeamPoints(origin, destination);
                core.SetPositions(beamPoints);
                glow.SetPositions(beamPoints);
            }
            else
            {
                core.SetPosition(0, origin);
                core.SetPosition(1, destination);
                glow.SetPosition(0, origin);
                glow.SetPosition(1, destination);
            }

            sparks.transform.position = destination;
            if (core.enabled)
                return;

            glow.enabled = true;
            core.enabled = true;
            sparks.Play(false);
        }

        /// <summary>
        /// 叠加两组横波并让偏移在光束两端归零
        /// </summary>
        private void UpdateBeamPoints(Vector3 origin, Vector3 destination)
        {
            Vector3 beam = destination - origin;
            Vector3 lateral = Vector3.Cross(Vector3.up, beam).normalized;
            if (lateral == Vector3.zero)
                lateral = Vector3.right;

            float phase = Time.time * waveFrequency * FullTurn;
            float amplitude = Mathf.Min(waveAmplitude, beam.magnitude * 0.12f);
            beamPoints[0] = origin;
            beamPoints[SegmentCount] = destination;
            for (int i = 1; i < SegmentCount; i++)
            {
                float progress = (float)i / SegmentCount;
                float envelope = Mathf.Sin(progress * Mathf.PI);
                float mainWave = Mathf.Sin(progress * FullTurn * 3f - phase);
                float detailWave = Mathf.Sin(progress * FullTurn * 8f + phase * 1.65f);
                float offset = amplitude * envelope * (mainWave * 0.65f + detailWave * 0.35f);
                beamPoints[i] = origin + beam * progress + lateral * offset;
            }
        }

        /// <summary>
        /// 关闭光束并等待已发出的火花自然消散
        /// </summary>
        internal void StopBeam()
        {
            if (!core.enabled)
                return;

            core.enabled = false;
            glow.enabled = false;
            sparks.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
