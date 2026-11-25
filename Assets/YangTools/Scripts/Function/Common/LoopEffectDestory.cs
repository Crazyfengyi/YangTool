/*
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-08-03 
*/
using UnityEngine;
namespace YangTools
{
    /// <summary>
    /// 循环特效删除--先停止 等播完再删除
    /// </summary>
    public class LoopEffectDestroy : MonoBehaviour
    {
        // 最大生命周期
        private float maxLifeTime;
        // 需要停止发射并等待消散的粒子组件
        ParticleSystem[] list;
        /// <summary>
        /// 缓存粒子组件并计算停止发射后需要等待的消散时间
        /// </summary>
        public void Awake()
        {
            list = transform.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < list.Length; i++)
            {
                float startLifetime = 0;
                switch (list[i].main.startLifetime.mode)
                {
                    case ParticleSystemCurveMode.Constant:
                        startLifetime = list[i].main.startLifetime.constant;
                        break;
                    case ParticleSystemCurveMode.Curve:
                        startLifetime = list[i].main.startLifetime.curve.length;
                        break;
                    case ParticleSystemCurveMode.TwoCurves:
                        startLifetime = list[i].main.startLifetime.curveMax.length;
                        break;
                    case ParticleSystemCurveMode.TwoConstants:
                        startLifetime = list[i].main.startLifetime.constantMax;
                        break;
                    default:
                        break;
                }
                float thisTime = list[i].main.duration + startLifetime;
                if (maxLifeTime < thisTime)
                {
                    maxLifeTime = thisTime;
                }
            }
        }
        /// <summary>
        /// 删除循环特效--先停止 等播完再删除
        /// </summary>
        public void DestoryLoopEffect()
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] != null)
                {
                    list[i].Stop();
                }
            }

            Destroy(gameObject, maxLifeTime);
        }
    }
}