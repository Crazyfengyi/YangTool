using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YangTools
{
    public partial class Extends
    {
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
        /// 尝试将键和值添加到字典中：如果不存在，才添加；存在，不添加也不抛导常
        /// </summary>
        public static Dictionary<TKey, TValue> TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key)) dict.Add(key, value);
            return dict;
        }

        /// <summary>
        /// 将键和值添加或替换到字典中：如果不存在，则添加；存在，则替换
        /// </summary>
        public static Dictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            dict[key] = value;
            return dict;
        }

        /// <summary>
        /// 获取与指定的键相关联的值，如果没有则返回输入的默认值
        /// </summary>
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            //return dict.ContainsKey(key) ? dict[key] : defaultValue;
            return dict.TryGetValue(key, out TValue a) ? a : defaultValue;
        }

        /// <summary>
        /// 向字典中批量添加键值对 未考虑线程
        /// </summary>
        /// <param name="replaceExisted">如果已存在，是否替换</param>
        public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> values, bool replaceExisted)
        {
            foreach (var item in values)
            {
                if (!dict.ContainsKey(item.Key) || replaceExisted)
                    dict[item.Key] = item.Value;
            }
            return dict;
        }

        /// <summary>
        /// 随机返回bool值
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static bool RandomBool(this Random random)
        {
            return random.NextDouble() > 0.5d;
        }

        /// <summary>
        /// 随机返回枚举--不知道手动枚举值是否有效需考验
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="random"></param>
        /// <returns></returns>
        public static T RandomEnum<T>(this Random random) where T : struct
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
        public static Int32 NextInt16(this Random random)
        {
            return BitConverter.ToInt32(random.NextBytes(4), 0);
        }
        /// <summary>
        /// 随机一个float
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextFloat(this Random random)
        {
            return BitConverter.ToSingle(random.NextBytes(4), 0);
        }

        /// <summary>
        /// 生成一个Byte[]，用随机数填充
        /// </summary>
        /// <param name="random"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] NextBytes(this Random random, int length)
        {
            var data = new byte[length];
            random.NextBytes(data);
            return data;
        }

        /// <summary>
        /// string是否为空
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// 字符串连接
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        /// <summary>
        /// 是否是Int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsInt(this string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        /// <summary>
        /// 转Int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ToInt(this string s)
        {
            if (int.TryParse(s, out int i))
            {
                return i;
            }

            return int.Parse(s); //让它抛出错误
            //return int.Parse(s);
        }

        /// <summary>
        /// 替换字符串里的html标签
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

        //正则表达式===========================
        public static bool IsMatch(this string s, string pattern)
        {
            if (s == null)
            {
                return false;
            }
            else
            {
                return Regex.IsMatch(s, pattern);
            }
        }

        public static string Match(this string s, string pattern)
        {
            if (s == null) return "";
            return Regex.Match(s, pattern).Value;
        }

        public static void Test3()
        {
            bool b = "12345".IsMatch(@"\d+");
            string s = "ldp615".Match("[a-zA-Z]+");
        }
        //========================================


        //=================需整理==================
        //随机时间
        public static DateTime NextDateTime(this Random random, DateTime minValue, DateTime maxValue)
        {
            long ticks = minValue.Ticks + (long)((maxValue.Ticks - minValue.Ticks) * random.NextDouble());
            return new DateTime(ticks);
        }
        public static DateTime NextDateTime(this Random random)
        {
            return NextDateTime(random, DateTime.MinValue, DateTime.MaxValue);
        }
    }
}
