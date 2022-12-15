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
        /// <summary>
        /// 判断事件列表中是否存在这个委托
        /// </summary>
        public static bool EventHaveFunction(Action<int> onClick, Action<int> func)
        {
            Delegate[] list = onClick.GetInvocationList();

            return Convert.ToBoolean(Array.IndexOf(list, func));
        }

        #region 序列化
        /// <summary>
        /// 利用反射实现深拷贝---对象的相互引用，会导致方法循环调用:建议使用序列化的形式来进行深拷贝
        /// </summary>
        public static T DeepCopyWithReflection<T>(T obj)
        {
            Type type = obj.GetType();

            // 如果是字符串或值类型则直接返回
            if (obj is string || type.IsValueType) return obj;

            //如果是数组
            if (type.IsArray)
            {
                Type elementType = Type.GetType(type.FullName.Replace("[]", string.Empty));
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(DeepCopyWithReflection(array.GetValue(i)), i);
                }

                return (T)Convert.ChangeType(copied, obj.GetType());
            }

            object retval = Activator.CreateInstance(obj.GetType());

            PropertyInfo[] properties = obj.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance | BindingFlags.Static);
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(obj, null);
                if (propertyValue == null)
                    continue;
                property.SetValue(retval, DeepCopyWithReflection(propertyValue), null);
            }

            return (T)retval;
        }
        /// <summary>
        /// 利用DataContractSerializer序列化和反序列化实现
        /// </summary>
        public static T DeepCopy<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(T));
                ser.WriteObject(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = ser.ReadObject(ms);
                ms.Close();
            }
            return (T)retval;
        }
        /// <summary>
        /// 利用二进制序列化和反序列实现
        /// </summary>
        public static T DeepCopyWithBinarySerialize<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                // 序列化成流
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                // 反序列化成对象
                retval = bf.Deserialize(ms);
                ms.Close();
            }

            return (T)retval;
        }
        /// <summary>
        /// 利用XML序列化和反序列化实现
        /// </summary>
        public static T DeepCopyWithXmlSerializer<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }

            return (T)retval;
        }
        #endregion

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
        #endregion
    }
}
