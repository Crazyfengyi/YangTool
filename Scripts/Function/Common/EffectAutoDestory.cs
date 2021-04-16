using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YangTools
{
    /// <summary>
    /// 自动删除特效--放在粒子父节点上
    /// </summary>
    public class EffectAutoDestory : MonoBehaviour
    {
        /// <summary>
        /// 最大生命周期
        /// </summary>
        private float maxLifeTime;
        /// <summary>
        /// 计时
        /// </summary>
        private float currentTime;
        /// <summary>
        /// 是否活着的
        /// </summary>
        private bool isSurvive = true;
        /// <summary>
        /// 是否开启自动删除
        /// </summary>
        private bool isOpen = true;

        public void Awake()
        {
            ParticleSystem[] list = transform.GetComponentsInChildren<ParticleSystem>(true);

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].main.loop)
                {
#if UNITY_EDITOR
                    Debug.LogError($"有重复播放的粒子，不能删除---物体名称:{gameObject.name} 粒子名称：{list[i].gameObject.name}");
#endif
                    isOpen = false;
                    return;
                }

                float thisTime = list[i].main.duration + list[i].main.startLifetime.constantMax;
                if (maxLifeTime < thisTime)
                {
                    maxLifeTime = thisTime;
                }
            }
        }

        public void Update()
        {
            if (!isOpen) return;
            if (!isSurvive) return;

            currentTime += Time.deltaTime;
            if (currentTime > maxLifeTime)
            {
                Destroy(gameObject);
                isSurvive = false;
            }
        }
    }
}