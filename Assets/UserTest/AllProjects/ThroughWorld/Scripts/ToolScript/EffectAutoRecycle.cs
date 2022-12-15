/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-11-29 
*/
using UnityEngine;
/// <summary>
/// 自动回收特效--放在粒子父节点上
/// </summary>
public class EffectAutoRecycle : MonoBehaviour
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
    /// 是否开启自动回收
    /// </summary>
    private bool isOpen = true;
    /// <summary>
    /// 对象池物体脚本
    /// </summary>
    public EffectObjectPoolItem effectObjectPoolItem;
    public void Init()
    {
        ParticleSystem[] list = transform.GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].main.loop)
            {
#if UNITY_EDITOR
                Debug.LogError($"有重复播放的粒子，不能删除---物体名称:{gameObject.name} 粒子名称：{list[i].gameObject.name}");
#endif
                LoopEffectDestory temp = gameObject.AddComponent<LoopEffectDestory>();
                Destroy(this);

                isOpen = false;
                return;
            }

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
    public void Update()
    {
        if (!isOpen) return;
        if (!isSurvive) return;

        currentTime += Time.deltaTime;
        if (currentTime > maxLifeTime)
        {
            if (effectObjectPoolItem != null) GameEffectManager.Instance.RecycleEffet(effectObjectPoolItem);
            isSurvive = false;
            Destroy(this);
        }
    }
}