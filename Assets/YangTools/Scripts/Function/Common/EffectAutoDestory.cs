using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 自动删除特效(放在粒子父节点上)
    /// </summary>
    public class EffectAutoDestroy : MonoBehaviour
    {
        // 最大生命周期
        private float maxLifeTime;
        // 计时
        private float currentTime;
        // 是否活着的
        private bool isSurvive = true;
        // 是否开启自动删除
        private bool isOpen = true;

        /// <summary>
        /// 统计非循环粒子的最长播放时间 遇到循环粒子时关闭自动销毁
        /// </summary>
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
        /// <summary>
        /// 在允许计时且存活时累积时间并于播放结束后销毁物体
        /// </summary>
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