using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace YangTools
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
            return (1 - t) * ((1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2)) + t * ((1 - t) * ((1 - t) * p1 + t * p2) + t * ((1 - t) * p2 + t * p3));
        }
        public static void Bezier_3ref(ref Vector3 outValue, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            outValue = (1 - t) * ((1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2)) + t * ((1 - t) * ((1 - t) * p1 + t * p2) + t * ((1 - t) * p2 + t * p3));
        }
        /// <summary>
        /// Catmull-Rom 曲线插值---返回点1-2的曲线
        /// </summary>
        /// <param name="P0">点0</param>
        /// <param name="P1">点1</param>
        /// <param name="P2">点2</param>
        /// <param name="P3">点3</param>
        /// <param name="t">百分比0-1</param>
        public static Vector3 CatmullRomPoint(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
        {
            float factor = 0.5f;
            Vector3 c0 = P1;
            Vector3 c1 = (P2 - P0) * factor;
            Vector3 c2 = (P2 - P1) * 3f - (P3 - P1) * factor - (P2 - P0) * 2f * factor;
            Vector3 c3 = (P2 - P1) * -2f + (P3 - P1) * factor + (P2 - P0) * factor;

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
        public static Vector3 [] GetBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint,int segmentNum)
        {
            Vector3 [] path = new Vector3[segmentNum];
            for (int i = 1; i <= segmentNum; i++)
            {
                float t = i / (float)segmentNum;
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
        private static  Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
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
        /// <param name="Forward">正前方</param>
        /// <param name="SecondValue">要判断的向量(点)</param>
        public static CheckDirectionType CheckDirection(Vector3 Forward, Vector3 SecondValue)
        {
            CheckDirectionType directionType = CheckDirectionType.None;
            //点乘--判断前后 >0:前 <0:后 
            /*
             * 大于0:表示两向量角度小于90度
             * 等于0:表示两向量垂直
             * 小于0:表示两向量角度大于90度
             */
            float dotVaule = Vector3.Dot(Forward.normalized, SecondValue.normalized);
            //叉乘--判断左右 y>0:左 y<0:右
            /*
             * 结果为一条新的向量,并垂直于两条旧的向量
             */
            Vector3 crossVaule = Vector3.Cross(Forward.normalized, SecondValue.normalized);

            //叉乘和点乘的结合可用来准确判断方位
            //前
            if (dotVaule > 0)
            {
                if (crossVaule.y > 0) //在左前方
                {
                    directionType = CheckDirectionType.LeftForward;
                }
                else if (crossVaule.y < 0)//在右前方
                {
                    directionType = CheckDirectionType.RightForward;
                }
                else
                {
                    directionType = CheckDirectionType.Forward;
                }
            }
            else if (dotVaule < 0)//后
            {
                if (crossVaule.y > 0) //在左后方
                {
                    directionType = CheckDirectionType.LeftBack;
                }
                else if (crossVaule.y < 0)//在右后方
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
                if (crossVaule.y > 0)//左
                {
                    directionType = CheckDirectionType.Left;

                }
                else if (crossVaule.y < 0)//右
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
    }
}
