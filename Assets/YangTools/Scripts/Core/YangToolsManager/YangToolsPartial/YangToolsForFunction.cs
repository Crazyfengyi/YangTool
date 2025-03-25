using System;
using UnityEngine;

namespace YangTools.Scripts.Core
{
    public static partial class YangToolsManager
    {
        #region 曲线方法

        /// <summary>
        /// 二次贝塞尔
        /// </summary>
        public static Vector3 Bezier_2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return (1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2);
        }

        public static void Bezier_2ref(ref Vector3 outValue, Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            outValue = (1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2);
        }

        /// <summary>
        /// 三次贝塞尔
        /// </summary>
        public static Vector3 Bezier_3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return (1 - t) * ((1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2)) +
                   t * ((1 - t) * ((1 - t) * p1 + t * p2) + t * ((1 - t) * p2 + t * p3));
        }

        public static void Bezier_3ref(ref Vector3 outValue, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            outValue = (1 - t) * ((1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2)) +
                       t * ((1 - t) * ((1 - t) * p1 + t * p2) + t * ((1 - t) * p2 + t * p3));
        }

        /// <summary>
        /// Catmull-Rom 曲线插值---返回点1-2的曲线
        /// </summary>
        /// <param name="p0">点0</param>
        /// <param name="p1">点1</param>
        /// <param name="p2">点2</param>
        /// <param name="p3">点3</param>
        /// <param name="t">百分比0-1</param>
        public static Vector3 CatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float factor = 0.5f;
            Vector3 c0 = p1;
            Vector3 c1 = (p2 - p0) * factor;
            Vector3 c2 = (p2 - p1) * 3f - (p3 - p1) * factor - (p2 - p0) * 2f * factor;
            Vector3 c3 = (p2 - p1) * -2f + (p3 - p1) * factor + (p2 - p0) * factor;

            Vector3 curvePoint = c3 * t * t * t + c2 * t * t + c1 * t + c0;
            return curvePoint;
        }

        /// <summary>
        /// 获取存储贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>起始点
        /// <param name="controlPoint"></param>控制点
        /// <param name="endPoint"></param>目标点
        /// <param name="segmentNum"></param>采样点的数量
        /// <returns>存储贝塞尔曲线点的数组</returns>
        public static Vector3[] GetBezierList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint,
            int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 1; i <= segmentNum; i++)
            {
                float t = i / (float) segmentNum;
                Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                    controlPoint, endPoint);
                path[i - 1] = pixel;
                //Debug.Log(path[i-1]);
            }

            return path;
        }

        /// <summary>
        /// 根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t">T值</param>
        /// <param name="p0">起始点</param>
        /// <param name="p1">控制点</param>
        /// <param name="p2">目标点</param>
        /// <returns>根据T值计算出来的贝赛尔曲线点</returns>
        private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;

            return p;
        }

        #endregion

        #region 判断左右

        /// <summary>
        /// 判断方向
        /// </summary>
        /// <param name="forward">正前方</param>
        /// <param name="secondValue">要判断的向量(点)</param>
        public static CheckDirectionType CheckDirection(Vector3 forward, Vector3 secondValue)
        {
            CheckDirectionType directionType = CheckDirectionType.None;
            //点乘--判断前后 >0:前 <0:后 
            /*
             * 大于0:表示两向量角度小于90度
             * 等于0:表示两向量垂直
             * 小于0:表示两向量角度大于90度
             */
            float dotValue = Vector3.Dot(forward.normalized, secondValue.normalized);
            //叉乘--判断左右 y>0:左 y<0:右
            /*
             * 结果为一条新的向量,并垂直于两条旧的向量
             */
            Vector3 crossValue = Vector3.Cross(forward.normalized, secondValue.normalized);

            //叉乘和点乘的结合可用来准确判断方位
            //前
            if (dotValue > 0)
            {
                if (crossValue.y > 0) //在左前方
                {
                    directionType = CheckDirectionType.LeftForward;
                }
                else if (crossValue.y < 0) //在右前方
                {
                    directionType = CheckDirectionType.RightForward;
                }
                else
                {
                    directionType = CheckDirectionType.Forward;
                }
            }
            else if (dotValue < 0) //后
            {
                if (crossValue.y > 0) //在左后方
                {
                    directionType = CheckDirectionType.LeftBack;
                }
                else if (crossValue.y < 0) //在右后方
                {
                    directionType = CheckDirectionType.RightBack;
                }
                else
                {
                    directionType = CheckDirectionType.Back;
                }
            }
            else
            {
                if (crossValue.y > 0) //左
                {
                    directionType = CheckDirectionType.Left;
                }
                else if (crossValue.y < 0) //右
                {
                    directionType = CheckDirectionType.Right;
                }
                else
                {
                    directionType = CheckDirectionType.Equal;
                }
            }

            return directionType;
        }

        //方向
        [Flags]
        public enum CheckDirectionType
        {
            None = 0,
            Equal = 1,
            Forward = 1 << 1,
            Back = 1 << 2,
            Left = 1 << 3,
            Right = 1 << 4,

            LeftForward = Left + Forward,
            LeftBack = Left + Back,
            RightForward = Right + Forward,
            RightBack = Right + Back,
        }

        #endregion

        #region 数学范围检测

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
        public static bool CurveRange(Transform self, Transform target, float minDistance, float maxDistance,
            float maxAngle)
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
        public static bool SquareRange(Transform self, Transform target, float maxWidth, float minHeight,
            float maxHeight)
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

            float angleDistance = maxDistance * Mathf.Cos(maxAngle * 0.5f * Mathf.Deg2Rad) /
                                  Mathf.Cos(angle * Mathf.Deg2Rad);
            float distance = Vector3.Distance(target.position, self.position);
            if (distance <= angleDistance)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}