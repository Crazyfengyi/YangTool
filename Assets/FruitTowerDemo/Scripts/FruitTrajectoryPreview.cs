using UnityEngine;

namespace FruitTowerDemo
{
    /// <summary>
    /// 显示水果从生成点到目标点的抛射路线
    /// </summary>
    public sealed class FruitTrajectoryPreview : MonoBehaviour
    {
        /// <summary>
        /// 水果塔游戏控制器
        /// </summary>
        [SerializeField] private FruitTowerGameController gameController;

        /// <summary>
        /// 用于绘制路线的线段组件
        /// </summary>
        [SerializeField] private LineRenderer lineRenderer;

        /// <summary>
        /// 路线采样点数量
        /// </summary>
        [SerializeField] private int sampleCount = 24;

        /// <summary>
        /// 缓存的路线采样点
        /// </summary>
        private Vector3[] samplePositions;

        private void Awake()
        {
            EnsureSamplePositions();
            Hide();
        }

        /// <summary>
        /// 根据目标点刷新抛射路线
        /// </summary>
        public void Refresh(Vector3 worldPoint)
        {
            if (gameController == null || lineRenderer == null)
            {
                Hide();
                return;
            }

            EnsureSamplePositions();
            if (!gameController.TryGetThrowTrajectory(worldPoint, samplePositions))
            {
                Hide();
                return;
            }

            lineRenderer.positionCount = samplePositions.Length;
            lineRenderer.SetPositions(samplePositions);
            lineRenderer.enabled = true;
        }

        /// <summary>
        /// 隐藏抛射路线
        /// </summary>
        public void Hide()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }

        private void EnsureSamplePositions()
        {
            int validSampleCount = Mathf.Max(2, sampleCount);
            if (samplePositions == null || samplePositions.Length != validSampleCount)
            {
                samplePositions = new Vector3[validSampleCount];
            }
        }

        private void OnValidate()
        {
            sampleCount = Mathf.Max(2, sampleCount);
        }
    }
}
