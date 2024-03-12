/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-19 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using YangTools.ObjectPool;

namespace YangTools
{
    public static partial class YangToolsManager
    {
        #region 随机数
        private static System.Random s_Random = new System.Random((int)DateTime.UtcNow.Ticks);
        /// <summary>
        /// 设置随机数种子
        /// </summary>
        /// <param name="seed">随机数种子</param>
        public static void SetSeed(int seed)
        {
            s_Random = new System.Random(seed);
        }
        /// <summary>
        /// 返回非负随机数
        /// </summary>
        /// <returns>[0,System.Int32.MaxValue)的32位带符号整数</returns>
        public static int GetRandom()
        {
            return s_Random.Next();
        }
        /// <summary>
        /// 返回一个小于所指定最大值的非负随机数。
        /// </summary>
        /// <param name="maxValue">maxValue必须大于等于零</param>
        /// <returns>[0,maxValue)的32位带符号整数</returns>
        public static int GetRandom(int maxValue)
        {
            return s_Random.Next(maxValue);
        }
        /// <summary>
        /// 返回一个指定范围内的随机数
        /// </summary>
        /// <param name="minValue">返回的随机数的下界,</param>
        /// <param name="maxValue">返回的随机数的上界,maxValue必须大于等于minValue</param>
        /// <returns>[minValue,maxValue)</returns>
        public static int GetRandom(int minValue, int maxValue)
        {
            return s_Random.Next(minValue, maxValue);
        }
        /// <summary>
        /// 返回0~1范围内的随机数
        /// </summary>
        /// <returns>[0,1)</returns>
        public static float GetRandom01()
        {
            return (float)GetRandomDouble();
        }
        /// <summary>
        /// 返回一个介于0.0和1.0之间的随机数
        /// </summary>
        /// <returns>[0,1)的双精度浮点数</returns>
        public static double GetRandomDouble()
        {
            return s_Random.NextDouble();
        }
        /// <summary>
        /// 用随机数填充指定字节数组的元素
        /// </summary>
        /// <param name="buffer">包含随机数的字节数组</param>
        public static void GetRandomBytes(byte[] buffer)
        {
            s_Random.NextBytes(buffer);
        }
        #endregion

        #region 字符串
        private const int StringBuilderCapacity = 1024;
        [ThreadStatic]
        private static StringBuilder s_CachedStringBuilder = null;
        /// <summary>
        /// 获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式</param>
        /// <param name="args">字符串参数</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(string format, params object[] args)
        {
            if (format == null)
            {
                throw new Exception("Format is invalid.");
            }

            if (args == null)
            {
                throw new Exception("Args is invalid.");
            }
            CheckCachedStringBuilder();
            s_CachedStringBuilder.Length = 0;
            s_CachedStringBuilder.AppendFormat(format, args);
            return s_CachedStringBuilder.ToString();
        }
        private static void CheckCachedStringBuilder()
        {
            if (s_CachedStringBuilder == null)
            {
                s_CachedStringBuilder = new StringBuilder(StringBuilderCapacity);
            }
        }
        #endregion

        #region 分割数
        /// <summary>
        /// 将一个数分割成几份
        /// </summary>
        /// <param name="_targetNum">目标数</param>
        /// <param name="_splitCount">分割成几份</param>
        /// <returns>分割符数(数组)</returns>
        public static int[] SplitNumber(int _targetNum, int _splitCount)
        {
            int targetNum = _targetNum;
            int[] arry = new int[_splitCount - 1];
            System.Random rand = new System.Random(DateTime.Now.Millisecond);
            for (int i = 0; i < arry.Length; i++)
            {
                arry[i] = rand.Next(targetNum);
            }
            Array.Sort(arry);
            // split数组中存放的就是最后分成10份的数，
            int[] split = new int[_splitCount];
            for (int i = 0; i < split.Length; i++)
            {
                if (i == 0)
                {
                    split[i] = arry[i] - 0;
                }
                else if (i == split.Length - 1)
                {
                    split[i] = targetNum - arry[i - 1];
                }
                else
                {
                    split[i] = arry[i] - arry[i - 1];
                }
            }
            return split;
        }
        /// <summary>
        /// 将一个数分割成几份
        /// </summary>
        /// <param name="total">总数</param>
        /// <param name="partNum">几份</param>
        /// <param name="maxFloatNumber">最大浮动值</param>
        public static int[] SplitTheNumber(int total, int partNum, int maxFloatNumber)
        {
            int[] results = new int[partNum];
            if (partNum == 0) return results;

            System.Random rand = new System.Random(DateTime.Now.Millisecond);
            int baseNumber = Mathf.FloorToInt((float)total / (float)partNum); //获取基数
            int result = 0; //用来存储结果值
            int minRandNum = baseNumber - maxFloatNumber;
            int maxRandNum = baseNumber + maxFloatNumber;
            for (int i = 1; i <= partNum - 1; i++)
            {
                result = rand.Next(minRandNum, maxRandNum); //在浮动范围内取一个随机数
                total = total - result; //从总数中减掉结果值
                results[i - 1] = result; //得到的结果值写入数组
            }
            results[results.Length - 1] = total; //最后剩下的值写入数组

            return results;
        }
        /// <summary>
        /// 判断是否有交集
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list1">列表1</param>
        /// <param name="list2">列表2</param>
        /// <returns></returns>
        public static bool IsArrayIntersection<T>(List<T> list1, List<T> list2)
        {
            List<T> t = list1.Distinct().ToList();//去重
            List<T> exceptArr = t.Except(list2).ToList();//不包括

            if (exceptArr.Count < t.Count)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region  时间戳和异步
        /// <summary>
        /// 时间转换为时间戳--UTC
        /// </summary>
        /// <return>毫秒</return>
        public static long DataTimeConvertToTimeStamp(DateTime dataTime = default)
        {
            if (dataTime == default) dataTime = DateTime.UtcNow;
            TimeSpan ts = dataTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long timeStampStr = Convert.ToInt64(ts.TotalMilliseconds);
            return timeStampStr;
        }
        /// <summary>
        /// 时间戳转换为时间--UTC
        /// </summary>
        /// <param name="timestamp">毫秒</param>
        public static DateTime TimestampConvertToDate(this long timestamp)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Utc);
            return dtStart.AddMilliseconds(timestamp);
        }

        /// <summary>
        /// 将异步操作改成await
        /// </summary>
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
        #endregion

        #region 功能
        /// <summary>
        /// 返回相机在Z=0时的实际视口大小
        /// </summary>
        public static Bounds GetCameraView(Transform transform, Camera useCamera)
        {
            float width, height;
            if (useCamera == null)
                return new Bounds();
            if (useCamera.orthographic)
            {
                height = useCamera.orthographicSize * 2;
                width = useCamera.aspect * height;
            }
            else
            {
                height = (Mathf.Tan(useCamera.fieldOfView / 2f * Mathf.Deg2Rad)) * transform.position.z * 2;
                width = height * useCamera.aspect;
            }

            return new Bounds(transform.position, new Vector3(Mathf.Abs(width), Mathf.Abs(height), 0));
        }

        private static PointerEventData _eventDataCurrentPosition;
        private static List<RaycastResult> _results;
        /// <summary>
        /// 鼠标是否在UI上
        /// </summary>
        public static bool IsOverUI()
        {
            _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            _results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
            return _results.Count > 0;
        }
        /// <summary>
        /// 获得Transform的世界坐标
        /// </summary>
        public static Vector3 GetWorldPosOfCanvasRectTransform(RectTransform trans, Camera camera = null)
        {
            if (camera == null) camera = Camera.main;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(trans, trans.position, camera, out Vector3 result);
            return result;
        }
        
        #endregion
    }
}