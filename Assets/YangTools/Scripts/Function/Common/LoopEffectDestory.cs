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
    /// 循环特效删除--先停止,等播完再删除
    /// </summary>
    public class LoopEffectDestroy : MonoBehaviour
    {
        /// <summary>
        /// 最大生命周期
        /// </summary>
        private float maxLifeTime;
        ParticleSystem[] list;
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
        /// 删除循环特效--先停止,等播完再删除
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