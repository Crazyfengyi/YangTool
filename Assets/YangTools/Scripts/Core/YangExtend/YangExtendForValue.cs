using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace YangTools.Extend
{
    public partial class YangExtend
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

        /// <summary>
        /// 克隆List
        /// </summary>
        public static List<T> GetClone<T>(this List<T> list)
        {
            List<T> newList = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                newList.Add(list[i]);
            }
            return newList;
        }
        /// <summary>
        /// 克隆类对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="RealObject"></param>
        /// <returns></returns>
        public static T Clone<T>(T RealObject)
        {
            using (Stream objStream = new MemoryStream())
            {
                //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objStream, RealObject);
                objStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objStream);
            }
        }

        #region Unity相关
        /// <summary>
        /// vector3转vector2
        /// </summary>
        /// <returns>去掉y</returns>
        public static Vector2 ToVector2(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }
        /// <summary>
        /// vector2转vector3
        /// </summary>
        /// <returns>y默认为0</returns>
        public static Vector3 ToVector3(this Vector2 vector2, float y = 0f)
        {
            return new Vector3(vector2.x, y, vector2.y);
        }
        /// <summary>
        /// 设置绝对位置的x坐标。
        /// </summary>
        /// <param name="newValue">x坐标值</param>
        public static void SetPositionX(this Transform transform, float newValue)
        {
            Vector3 v = transform.position;
            v.x = newValue;
            transform.position = v;
        }
        /// <summary>
        /// 设置绝对位置的y坐标。
        /// </summary>
        /// <param name="newValue">y坐标值</param>
        public static void SetPositionY(this Transform transform, float newValue)
        {
            Vector3 v = transform.position;
            v.y = newValue;
            transform.position = v;
        }
        /// <summary>
        /// 设置绝对位置的z坐标。
        /// </summary>
        /// <param name="newValue">z坐标值</param>
        public static void SetPositionZ(this Transform transform, float newValue)
        {
            Vector3 v = transform.position;
            v.z = newValue;
            transform.position = v;
        }
        /// <summary>
        /// 增加绝对位置的x坐标。
        /// </summary>
        /// <param name="deltaValue">x坐标值增量</param>
        public static void AddPositionX(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.position;
            v.x += deltaValue;
            transform.position = v;
        }
        /// <summary>
        /// 增加绝对位置的y坐标。
        /// </summary>
        /// <param name="deltaValue">y坐标值增量</param>
        public static void AddPositionY(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.position;
            v.y += deltaValue;
            transform.position = v;
        }
        /// <summary>
        /// 增加绝对位置的 z 坐标。
        /// </summary>
        /// <param name="deltaValue">z坐标值增量</param>
        public static void AddPositionZ(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.position;
            v.z += deltaValue;
            transform.position = v;
        }

        /// <summary>
        /// 设置相对位置的x坐标。
        /// </summary>
        /// <param name="newValue">x坐标值</param>
        public static void SetLocalPositionX(this Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.x = newValue;
            transform.localPosition = v;
        }
        /// <summary>
        /// 设置相对位置的y坐标。
        /// </summary>
        /// <param name="newValue">y坐标值</param>
        public static void SetLocalPositionY(this Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.y = newValue;
            transform.localPosition = v;
        }
        /// <summary>
        /// 设置相对位置的z坐标。
        /// </summary>
        /// <param name="newValue">z坐标值</param>
        public static void SetLocalPositionZ(this Transform transform, float newValue)
        {
            Vector3 v = transform.localPosition;
            v.z = newValue;
            transform.localPosition = v;
        }
        /// <summary>
        /// 增加相对位置的x坐标。
        /// </summary>
        /// <param name="deltaValue">x坐标值</param>
        public static void AddLocalPositionX(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.x += deltaValue;
            transform.localPosition = v;
        }
        /// <summary>
        /// 增加相对位置的y坐标。
        /// </summary>
        /// <param name="deltaValue">y坐标值</param>
        public static void AddLocalPositionY(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.y += deltaValue;
            transform.localPosition = v;
        }
        /// <summary>
        /// 增加相对位置的z坐标。
        /// </summary>
        /// <param name="deltaValue">z坐标值</param>
        public static void AddLocalPositionZ(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localPosition;
            v.z += deltaValue;
            transform.localPosition = v;
        }

        /// <summary>
        /// 设置相对尺寸的x分量。
        /// </summary>
        /// <param name="newValue">x分量值</param>
        public static void SetLocalScaleX(this Transform transform, float newValue)
        {
            Vector3 v = transform.localScale;
            v.x = newValue;
            transform.localScale = v;
        }
        /// <summary>
        /// 设置相对尺寸的y分量。
        /// </summary>
        /// <param name="newValue">y分量值</param>
        public static void SetLocalScaleY(this Transform transform, float newValue)
        {
            Vector3 v = transform.localScale;
            v.y = newValue;
            transform.localScale = v;
        }
        /// <summary>
        /// 设置相对尺寸的z分量。
        /// </summary>
        /// <param name="newValue">z分量值</param>
        public static void SetLocalScaleZ(this Transform transform, float newValue)
        {
            Vector3 v = transform.localScale;
            v.z = newValue;
            transform.localScale = v;

        }
        /// <summary>
        /// 增加相对尺寸的x分量。
        /// </summary>
        /// <param name="deltaValue">x分量增量</param>
        public static void AddLocalScaleX(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localScale;
            v.x += deltaValue;
            transform.localScale = v;
        }
        /// <summary>
        /// 增加相对尺寸的y分量。
        /// </summary>
        /// <param name="deltaValue">y分量增量</param>
        public static void AddLocalScaleY(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localScale;
            v.y += deltaValue;
            transform.localScale = v;
        }
        /// <summary>
        /// 增加相对尺寸的z分量。
        /// </summary>
        /// <param name="deltaValue">z分量增量</param>
        public static void AddLocalScaleZ(this Transform transform, float deltaValue)
        {
            Vector3 v = transform.localScale;
            v.z += deltaValue;
            transform.localScale = v;
        }

        #endregion
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
