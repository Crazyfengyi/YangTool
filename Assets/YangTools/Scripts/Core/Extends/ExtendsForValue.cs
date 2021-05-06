using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace YangTools
{
    public partial class Extends
    {
        /// <summary>
        /// 检查字段是否包含某个特性
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        public static bool HasAttribute<T>(this FieldInfo fieldInfo) where T : System.Attribute
        {
            try
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(T), false).Cast<T>();
                return attrs.First() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 是否为2的N次幂
        /// </summary>
        /// <param name="num">自身(int)</param>
        /// <returns></returns>
        public static bool IsPowerOfTwo(this int num)
        {
            //这些数减1后与自身进行按位与，如果结果为0，表示这个数是2的n次幂
            return num > 0 && (num & (num - 1)) == 0;
        }

        /// <summary>
        /// 获取大于N的最小的2的N次方(如果本身是2的N次幂就返回)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int GetThanPowerOfTwo(int n)
        {
            //如果本身是2的N次幂就返回
            if (IsPowerOfTwo(n)) return n;
            //int 最大32位
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;

            n += 1;  //大于N的最小的2的N次方
            return n;
        }

        /// <summary>
        /// 获取小于于N的最大的2的N次方(如果本身是2的N次幂就返回)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int GetUnderPowerOfTwo(int n)
        {
            //如果本身是2的N次幂就返回
            if (IsPowerOfTwo(n)) return n;
            //int 最大32位
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;

            n += 1;  //大于N的最小的2的N次方
            n = n >> 1; //小于N的最大的2的N次方

            return n;
        }

        /// <summary>
        /// 判断字符串是否为空
        /// </summary> 
        public static bool IsNull(this string str)
        {
            //extendName.Lengh效率最高，但必须判断有初始化---string str; 或者 string str = null 会报错;
            return str == null || str?.Length == 0 || string.IsNullOrEmpty(str) || str == string.Empty;
        }

        /// <summary>
        /// 随机返回bool值
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static bool RandomBool(this System.Random random)
        {
            return random.NextDouble() > 0.5d;
        }

        /// <summary>
        /// 随机返回枚举--不知道手动枚举值是否有效，需验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="random"></param>
        /// <returns></returns>
        public static T RandomEnum<T>(this System.Random random) where T : struct
        {
            Type type = typeof(T);
            if (type.IsEnum == false) throw new InvalidOperationException();

            Array array = Enum.GetValues(type);
            int index = random.Next(array.GetLowerBound(0), array.GetUpperBound(0) + 1);
            return (T)array.GetValue(index);
        }

        /// <summary>
        /// 随机一个int32
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Int32 NextInt16(this System.Random random)
        {
            return BitConverter.ToInt32(random.NextBytes(4), 0);
        }
        /// <summary>
        /// 随机一个float
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextFloat(this System.Random random)
        {
            return BitConverter.ToSingle(random.NextBytes(4), 0);
        }

        /// <summary>
        /// 生成一个Byte[]，用随机数填充
        /// </summary>
        /// <param name="random"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] NextBytes(this System.Random random, int length)
        {
            var data = new byte[length];
            random.NextBytes(data);
            return data;
        }

        /// <summary>
        /// 去掉字符串的html标签
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ContentReplace(this string input)
        {
            input = Regex.Replace(input, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"-->", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"<!--.*", "", RegexOptions.IgnoreCase);

            input = Regex.Replace(input, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            input.Replace("<", "");
            input.Replace(">", "");
            input.Replace("\r\n", "");
            //去两端空格，中间多余空格
            input = Regex.Replace(input.Trim(), "\\s+", " ");
            return input;
        }

        //=================需整理+验证==================
        //随机时间
        public static DateTime NextDateTime(this System.Random random, DateTime minValue, DateTime maxValue)
        {
            long ticks = minValue.Ticks + (long)((maxValue.Ticks - minValue.Ticks) * random.NextDouble());
            return new DateTime(ticks);
        }
        public static DateTime NextDateTime(this System.Random random)
        {
            return NextDateTime(random, DateTime.MinValue, DateTime.MaxValue);
        }

        #region 根据权重获取

        /// <summary>
        /// 从权重信息组里根据权重随机一个
        /// </summary>
        /// <typeparam name="T">权重类</typeparam>
        /// <param name="weightInfos">权重组</param>
        /// <param name="isRemoveWhenFind">是否在找到时从容器移除(需要容器允许该操作)</param>
        public static T GetRandomInfo<T>(IEnumerable<T> weightInfos, bool isRemoveWhenFind = false)
            where T : IWeight<T>
        {
            T ret = default;

            int totalWeight = 0;//总权重值
            foreach (T i in weightInfos)
            {
                totalWeight += i.GetWeight();
            }

            if (totalWeight == 0) return ret;
            int indexWeight = 0;
            int randomWeight = UnityEngine.Random.Range(1, totalWeight + 1);

            foreach (T item in weightInfos)
            {
                indexWeight += item.GetWeight();
                if (randomWeight <= indexWeight)
                {
                    ret = item;
                    if (isRemoveWhenFind)
                    {
                        ICollection<T> collection = weightInfos as ICollection<T>;
                        collection?.Remove(item);
                    }
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// 权重接口
        /// </summary>
        public interface IWeight<T>
        {
            /// <summary>
            /// 获得权重
            /// </summary>
            /// <returns>权重值</returns>
            public int GetWeight();
            /// <summary>
            /// 获得类型
            /// </summary>
            /// <returns>类型</returns>
            public T GetItem();
        }

        #endregion
    }
}
