using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 抛物线导弹
    /// <para>计算弹道和转向</para>
    /// </summary>
    public class Missile : MonoBehaviour
    {
        public Transform target;        // 目标
        public float height = 16f;      // 高度
        public float gravity = -9.8f;   // 重力加速度
        public ParabolaPath path;      // 抛物线运动轨迹

        private void Start()
        {
            path = new ParabolaPath(transform.position, target.position, Random.Range(10, 16), gravity);
            path.isClampStartEnd = true;
            transform.LookAt(path.GetPosition(path.Time + Time.deltaTime));
        }

        private void Update()
        {
            // 计算位移
            float t = Time.deltaTime;
            path.Time += t;
            transform.position = path.Position;

            // 计算转向
            transform.LookAt(path.GetPosition(path.Time + t));

            // 简单模拟一下碰撞检测
            if (path.Time >= path.TotalTime)
            {
                enabled = false;
            }
        }

    }
}