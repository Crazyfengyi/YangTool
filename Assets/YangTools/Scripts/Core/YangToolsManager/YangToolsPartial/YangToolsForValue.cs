/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-19 
*/
using System;
using System.Text;

namespace YangTools
{
    public static partial class YangToolsManager
    {
        #region 随机数
        private static System.Random s_Random = new System.Random((int)DateTime.UtcNow.Ticks);
        /// <summary>
        /// 设置随机数种子。
        /// </summary>
        /// <param name="seed">随机数种子。</param>
        public static void SetSeed(int seed)
        {
            s_Random = new System.Random(seed);
        }
        /// <summary>
        /// 返回非负随机数。
        /// </summary>
        /// <returns>[0,System.Int32.MaxValue)的32 位带符号整数。</returns>
        public static int GetRandom()
        {
            return s_Random.Next();
        }
        /// <summary>
        /// 返回一个小于所指定最大值的非负随机数。
        /// </summary>
        /// <param name="maxValue">maxValue必须大于等于零。</param>
        /// <returns>[0,maxValue)的32位带符号整数</returns>
        public static int GetRandom(int maxValue)
        {
            return s_Random.Next(maxValue);
        }
        /// <summary>
        /// 返回一个指定范围内的随机数。
        /// </summary>
        /// <param name="minValue">返回的随机数的下界,</param>
        /// <param name="maxValue">返回的随机数的上界,maxValue必须大于等于minValue.</param>
        /// <returns>[minValue,maxValue)</returns>
        public static int GetRandom(int minValue, int maxValue)
        {
            return s_Random.Next(minValue, maxValue);
        }
        /// <summary>
        /// 返回0~1范围内的随机数。
        /// </summary>
        /// <returns>[0,1)</returns>
        public static float GetRandom01()
        {
            return (float)GetRandomDouble();
        }
        /// <summary>
        /// 返回一个介于0.0和1.0之间的随机数。
        /// </summary>
        /// <returns>[0,1)的双精度浮点数。</returns>
        public static double GetRandomDouble()
        {
            return s_Random.NextDouble();
        }
        /// <summary>
        /// 用随机数填充指定字节数组的元素。
        /// </summary>
        /// <param name="buffer">包含随机数的字节数组.</param>
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

        #region 参考
        ///// <summary>
        ///// 是否有泛型接口
        ///// </summary>
        //private static bool HaveInterface(Type type)
        //{
        //    return Array.Exists(type.GetInterfaces(), t =>
        //    {
        //        if (t.IsGenericType)
        //        {
        //            return t.GetGenericTypeDefinition() == typeof(IPoolItem<>);
        //        }
        //        return false;
        //    });
        //}
        #endregion
    }
}