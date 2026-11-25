using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 延时删除TrailRender
    /// </summary>
    public class WaitLineDestroy : MonoBehaviour
    {
        // 需要等待消散的拖尾组件
        private TrailRenderer[] trailRenderers;

        private float timer; //删除延时
        /// <summary>
        /// 缓存拖尾组件并统计最长拖尾消散时间
        /// </summary>
        private void Awake()
        {
            trailRenderers = GetComponentsInChildren<TrailRenderer>();

            for (int i = 0; i < trailRenderers.Length; i++)
            {
                if (trailRenderers[i].time > timer)
                {
                    timer = trailRenderers[i].time;
                }
            }
        }

        /// <summary>
        /// 根据拖尾延时删除
        /// </summary>
        public void DelayDestroy()
        {
            Destroy(gameObject, timer);
        }
    }
}