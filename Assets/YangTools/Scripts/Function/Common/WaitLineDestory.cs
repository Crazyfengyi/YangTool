using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 延时删除TrailRender
    /// </summary>
    public class WaitLineDestory : MonoBehaviour
    {
        private TrailRenderer[] trailRenderers;

        private float timer; //删除延时
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