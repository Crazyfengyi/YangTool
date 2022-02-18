using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace YangTools.Extend
{
    public partial class YangExtend
    {
        /// <summary>
        /// 在物体世界坐标播放声音
        /// </summary>
        /// <param name="clipName">音效名称</param>
        public static void PlayAtPoint(this GameObject obj, AudioClip clipName)
        {
            AudioSource.PlayClipAtPoint(clipName, obj.transform.position);
        }

        /// <summary>
        /// 获得组件，如果没有就添加一个
        /// </summary>
        /// <typeparam name="T">组件(必须继承自Behaviour)</typeparam>
        /// <returns>组件</returns>
        public static T GetComponent_Extend<T>(this GameObject obj) where T : Behaviour
        {
            T scrpitType = obj.transform.GetComponent<T>();

            if (scrpitType == null)
            {
                scrpitType = obj.AddComponent<T>();
            }

            return scrpitType;
        }

        /// <summary>
        /// 刷新自己和子节点的ContentSizeFitter(倒序刷新)
        /// </summary>
        /// <param name="node">目标节点</param>
        public static void RefreshAllContentSizeFitter(this GameObject node)
        {
            ContentSizeFitter[] allComponent = node.transform.GetComponentsInChildren<ContentSizeFitter>(true);

            for (int i = allComponent.Length - 1; i >= 0; i--)
            {
                RectTransform rectTrans = allComponent[i].GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTrans);
            }
        }

        /// <summary>
        /// 刷新自身和父节点ContentSizeFitter(直至根节点)
        /// </summary>
        /// <param name="node">目标节点</param>
        public static void UpdateContent(this GameObject node)
        {
            Transform content = node.transform;
            while (content != null)
            {
                if (content.TryGetComponent<ContentSizeFitter>(out ContentSizeFitter fitter))
                {
                    RectTransform rectTrans = content.GetComponent<RectTransform>();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTrans);
                }
                content = content.parent;
            }
        }
    }
    /// <summary>
    /// 范围检测类
    /// </summary>
    public static class RangeCheck
    {
        /// <summary>
        /// 扇形检测
        /// </summary>
        /// <param name="self">原点</param>
        /// <param name="target">检测目标</param>
        /// <param name="maxDistance">最大距离</param>
        /// <param name="maxAngle">检测角度</param>
        /// <returns>是否在扇形内</returns>
        public static bool CurveRange(Transform self, Transform target, float maxDistance, float maxAngle)
        {
            return CurveRange(self, target, 0, maxDistance, maxAngle);
        }
        /// <summary>
        /// 扇形
        /// </summary>
        public static bool CurveRange(Transform self, Transform target, float minDistance, float maxDistance, float maxAngle)
        {
            Vector3 playerDir = self.forward;
            Vector3 enemydir = (target.position - self.position).normalized;
            float angle = Vector3.Angle(playerDir, enemydir);
            if (angle > maxAngle * 0.5f)
            {
                return false;
            }
            float distance = Vector3.Distance(target.position, self.position);
            if (distance <= maxDistance && distance >= minDistance)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 圆形检测
        /// </summary>
        /// <param name="self">原点</param>
        /// <param name="target">检测目标</param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns>是否在圆形内</returns>
        public static bool CircleRange(Transform self, Transform target, float maxDistance)
        {
            return CircleRange(self, target, 0, maxDistance);
        }
        /// <summary>
        /// 圆形
        /// </summary>
        public static bool CircleRange(Transform self, Transform target, float minDistance, float maxDistance)
        {
            float distance = Vector3.Distance(target.position, self.position);
            if (distance <= maxDistance && distance >= minDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 矩形检测
        /// </summary>
        /// <param name="self">原点</param>
        /// <param name="target">检测目标</param>
        /// <param name="maxWidth">最大宽度</param>
        /// <param name="maxHeight">最大高度</param>
        /// <returns></returns>
        public static bool SquareRange(Transform self, Transform target, float maxWidth, float maxHeight)
        {
            return SquareRange(self, target, maxWidth, 0, maxHeight);
        }
        /// <summary>
        /// 矩形
        /// </summary>
        public static bool SquareRange(Transform self, Transform target, float maxWidth, float minHeight, float maxHeight)
        {
            Vector3 enemyDir = (target.position - self.position).normalized;
            float angle = Vector3.Angle(enemyDir, self.forward);
            if (angle > 90)
            {
                return false;
            }
            float distance = Vector3.Distance(target.position, self.position);
            float z = distance * Mathf.Cos(angle * Mathf.Deg2Rad);
            float x = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
            if (x <= maxWidth * 0.5f && z <= maxHeight && z >= minHeight)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 等腰三角形检测
        /// </summary>
        /// <param name="self">原点</param>
        /// <param name="target">检测目标</param>
        /// <param name="maxDistance">最大距离</param>
        /// <param name="maxAngle">检测角度</param>
        /// <returns></returns>
        public static bool TriangleRange(Transform self, Transform target, float maxDistance, float maxAngle)
        {
            Vector3 playerDir = self.forward;
            Vector3 enemydir = (target.position - self.position).normalized;
            float angle = Vector3.Angle(playerDir, enemydir);
            if (angle > maxAngle * 0.5f)
            {
                return false;
            }
            float angleDistance = maxDistance * Mathf.Cos(maxAngle * 0.5f * Mathf.Deg2Rad) / Mathf.Cos(angle * Mathf.Deg2Rad);
            float distance = Vector3.Distance(target.position, self.position);
            if (distance <= angleDistance)
            {
                return true;
            }
            return false;
        }
    }
}
