/** 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION# 
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-08-27 
*/
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LitJson;
using System.Reflection;
using System.Linq;
using YangTools;
using YangTools.Timer;

namespace YangTools
{
    /// <summary>
    /// 单集合多集合字段排序特性
    /// 使用方法：在字段上面加 [Ordinal(n)]，其中n可以是int任意数值，n越小，排序越靠前
    /// </summary>
    public class OrdinalAttribute : System.Attribute
    {
        public int Ordinal { get; set; }

        public OrdinalAttribute(int ordinal)
        {
            Ordinal = ordinal;
        }
    }

    /// <summary>
    /// 字段从表中加载的进度信息
    /// </summary>
    public class FieldLoadProgressInfo
    {
        /// <summary>
        /// 属性：当前被操作的对象实例
        /// </summary>
        public object ObjectInstance { get; set; }

        /// <summary>
        /// 属性：当前进度是否是“完成通知”
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// 属性：对象当前需赋值的字段信息
        /// </summary>
        public FieldInfo NowFieldInfo { get; set; }

        /// <summary>
        /// 属性：表中该字段的键
        /// </summary>
        public string TableKey { get; set; }

        /// <summary>
        /// 属性：表中该字段的值
        /// </summary>
        public string TableValue { get; set; }

        /// <summary>
        /// 通过现有信息，自动对结构体的字段进行赋值
        /// </summary>
        /// <param name="isStrId">是否将id字段通过字符id的方式进行赋值</param>
        public void AutoSetTableValueToFieldInfo(bool isStrId = false)
        {
            if (ObjectInstance == null) return;

            if (TableKey == "id")
            {
                if (isStrId)
                {
                    NowFieldInfo.SetValue(ObjectInstance, TableValue);
                }
                else
                {
                    NowFieldInfo.SetValue(ObjectInstance, int.Parse(TableValue));
                }
            }
            else
            {
                object universalValue = DataCore.ConvertStringToUniversalValue(TableValue, NowFieldInfo.FieldType);
                NowFieldInfo.SetValue(ObjectInstance, universalValue);
            }
        }
    }

    /// <summary>
    /// 具备包容性的json信息
    /// </summary>
    public class ToleranceJsonData
    {
        /// <summary>
        /// 可以直接赋值和获得的JsonData
        /// </summary>
        public JsonData jsonData = null;

        /// <summary>
        /// 构建空的信息，空信息调用HasError一定会返回true
        /// </summary>
        public ToleranceJsonData() { }

        /// <summary>
        /// 从JsonData构建
        /// </summary>
        /// <param name="json">JsonData源</param>
        public ToleranceJsonData(JsonData json)
        {
            jsonData = json;
        }

        /// <summary>
        /// 从JsonData进行隐式转化
        /// </summary>
        /// <param name="json">JsonData源</param>
        public static implicit operator ToleranceJsonData(JsonData json)
        {
            return new ToleranceJsonData(json);
        }

        /// <summary>
        /// 该信息是否有错误或为空
        /// </summary>
        public bool HasError
        {
            get
            {
                return jsonData == null;
            }
        }

        /// <summary>
        /// 信息是否是字典类型
        /// </summary>
        public bool IsMap
        {
            get
            {
                return HasError ? false : jsonData.IsObject;
            }
        }

        /// <summary>
        /// 信息是否是数组类型
        /// </summary>
        public bool IsArray
        {
            get
            {
                return HasError ? false : jsonData.IsArray;
            }
        }

        /// <summary>
        /// 信息是否是值类型
        /// </summary>
        public bool IsValue
        {
            get
            {
                return HasError ? false : (!jsonData.IsObject && !jsonData.IsArray);
            }
        }

        /// <summary>
        /// 转换到bool
        /// </summary>
        public bool AsBool()
        {
            if (!IsValue) return false;

            var s = jsonData.ToString();
            if (string.IsNullOrEmpty(s) || s == "0" || s == "false")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 转换到int
        /// </summary>
        public int AsInt()
        {
            if (!IsValue) return 0;

            int result;
            int.TryParse(jsonData.ToString(), out result);
            return result;
        }

        /// <summary>
        /// 转换到float
        /// </summary>
        public float AsFloat()
        {
            if (!IsValue) return 0;

            float result;
            float.TryParse(jsonData.ToString(), out result);
            return result;
        }

        /// <summary>
        /// 转换到double
        /// </summary>
        public double AsDouble()
        {
            if (!IsValue) return 0;

            double result;
            double.TryParse(jsonData.ToString(), out result);
            return result;
        }

        /// <summary>
        /// 转换到long
        /// </summary>
        public long AsLong()
        {
            if (!IsValue) return 0;

            long result;
            long.TryParse(jsonData.ToString(), out result);
            return result;
        }

        /// <summary>
        /// 转换到string
        /// </summary>
        public string AsString()
        {
            if (!IsValue) return "";

            var ret = jsonData.ToString();
            if (ret == null) ret = "";
            return ret;
        }

        /// <summary>
        /// 通过string键取Map的值，如果有错，返回有错的信息
        /// </summary>
        /// <param name="key">string键</param>
        public ToleranceJsonData this[string key]
        {
            get
            {
                if (!IsMap /*|| !jsonData.ContainsKey(key)*/) return new ToleranceJsonData();
                return new ToleranceJsonData(jsonData[key]);
            }
        }
    }

    #region 容器序列化处理器
    /// <summary>
    /// List 容器序列化处理器
    /// </summary>
    [System.Serializable]
    public class SerializationList<T>
    {
        [SerializeField]
        private List<T> target;
        public List<T> ToList() { return target; }

        public SerializationList(List<T> _target)
        {
            this.target = _target;
        }
    }

    /// <summary>
    /// HashSet 容器序列化处理器
    /// </summary>
    [System.Serializable]
    public class SerializationHashSet<T>
    {
        [SerializeField]
        private List<T> target;
        public HashSet<T> ToHashSet()
        {
            var ret = new HashSet<T>();
            foreach (var i in target)
            {
                ret.Add(i);
            }
            return ret;
        }

        public SerializationHashSet(HashSet<T> _target)
        {
            this.target = _target.ToList();
        }
    }

    /// <summary>
    /// Dictionary 容器序列化处理器
    /// </summary>
    [System.Serializable]
    public class SerializationDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        List<TKey> keys;
        [SerializeField]
        List<TValue> values;

        Dictionary<TKey, TValue> target;

        public Dictionary<TKey, TValue> ToDictionary() { return target; }

        public SerializationDictionary(Dictionary<TKey, TValue> _target)
        {
            this.target = _target;
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(target.Keys);
            values = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = System.Math.Min(keys.Count, values.Count);
            target = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; ++i)
            {
                target.Add(keys[i], values[i]);
            }
        }
    }

    /// <summary>
    /// SortedDictionary 容器序列化处理器
    /// </summary>
    [System.Serializable]
    public class SerializationSortedDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        List<TKey> keys;
        [SerializeField]
        List<TValue> values;

        SortedDictionary<TKey, TValue> target;

        public SortedDictionary<TKey, TValue> ToSortedDictionary() { return target; }

        public SerializationSortedDictionary(SortedDictionary<TKey, TValue> _target)
        {
            this.target = _target;
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(target.Keys);
            values = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = System.Math.Min(keys.Count, values.Count);
            target = new SortedDictionary<TKey, TValue>();
            for (var i = 0; i < count; ++i)
            {
                target.Add(keys[i], values[i]);
            }
        }
    }
    #endregion

    /// <summary>
    /// 数据处理器
    /// 使用Save一类的函数时，Key请不要传入特殊字符，不然无法形成文件名而导致无法保存
    /// </summary>
    public static class DataCore
    {
        #region 扩展方法
        /// <summary>
        /// 获取字段特性规定的顺序（不能获取返回0），用于字段排序
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        public static int GetOrdinal(this FieldInfo fieldInfo)
        {
            try
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(OrdinalAttribute), false).Cast<OrdinalAttribute>();
                return attrs.First().Ordinal;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 检查字段是否包含某个特性
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        public static bool HasAttribute<T>(this FieldInfo fieldInfo)
            where T : System.Attribute
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
        /// 字符串转换到任意类型（转换失败抛出错误）
        /// </summary>
        /// <typeparam name="T">需要转换到的类型</typeparam>
        /// <param name="str">需要转换的表格字符串</param>
        public static T ParseTo<T>(this string str)
        {
            return (T)DataCore.ConvertStringToUniversalValue(str, typeof(T));
        }
        #endregion

        #region 变量和初始化
        /// <summary>
        /// 数据储存默认路径
        /// </summary>
        public static readonly string dataCoderDirPath;
        /// <summary>
        /// 所有的key
        /// </summary>
        private readonly static HashSet<string> allKeys;
        /// <summary>
        /// key保存文件名
        /// </summary>
        private readonly static string keySavePath = "_All_Key";
        /// <summary>
        /// key备份文件名
        /// </summary>
        private readonly static string keySavePathBackup = "_All_Key_Backup";
        /// <summary>
        /// 保存key的字典
        /// </summary>
        private static bool saveKeyDirty = false;
        /// <summary>
        /// 最后一个Key字符串
        /// </summary>
        private static string lastKeyString = "";
        /// <summary>
        /// 数据文件转换格式
        /// </summary>
        public readonly static Encoding dataEncoding;
        /// <summary>
        /// 是否开启保存
        /// </summary>
        public static bool enableSave = true;

        /// <summary>
        /// 删除内部key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool DeleteInternalKey(string key)
        {
            if (!allKeys.Remove(key)) return false;
            SaveInternalKeys();
            return true;
        }

        /// <summary>
        /// 添加内部key
        /// </summary>
        /// <param name="key"></param>
        public static void AddInternalKey(string key)
        {
            if (!allKeys.Add(key)) return;
            SaveInternalKeys();
        }

        /// <summary>
        /// 清空内部key
        /// </summary>
        private static void ClearInternalKeys()
        {
            allKeys.Clear();
            SaveInternalKeys();
        }

        /// <summary>
        /// 保存内部key
        /// </summary>
        private static void SaveInternalKeys()
        {
            saveKeyDirty = true;
        }

        /// <summary>
        /// 检查是否保存key列表
        /// </summary>
        public static void CheckDirtyToSaveKeys()
        {
            if (saveKeyDirty)
            {
                saveKeyDirty = false;

                if (allKeys.Count == 0)
                {
                    RemoveFile(keySavePath);
                    RemoveFile(keySavePathBackup);
                }
                else
                {
                    lastKeyString = string.Join("|", allKeys);
                    if (WriteStringToFile(lastKeyString, keySavePath))
                    {
                        WriteStringToFile(lastKeyString, keySavePathBackup);
                    }
                    else
                    {
                        YangTimerManager.AddSecondTimer(0, () =>
                        {
                            WriteStringToFile(lastKeyString, keySavePathBackup);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 重新读取存档链表allKeys列表
        /// </summary>
        public static void ReloadInternalKeys()
        {
            allKeys.Clear();
            string saveKeyStr = GetStringFromFile(keySavePath);
            if (saveKeyStr == null || saveKeyStr.Length == 0)
            {
                //开始使用备份文件恢复存档
                saveKeyStr = GetStringFromFile(keySavePathBackup);
                if (saveKeyStr == null || saveKeyStr.Length == 0)
                {
                    Debug.LogError("allKeys从备份文件读取失败");
                    return;
                }
            }
            else if (!File.Exists(dataCoderDirPath + keySavePathBackup))//如果没有备份就进行备份
            {
                File.WriteAllText(dataCoderDirPath + keySavePathBackup, saveKeyStr);
            }

            //将所有Key放入HashSet
            string[] tempKeys = saveKeyStr.Split('|');
            for (int index = 0; index < tempKeys.Length; index++)
            {
                string i = tempKeys[index];
                if (i.Length > 0)
                {
                    allKeys.Add(i);
                }
            }
        }
        /// <summary>
        /// 静态初始化
        /// </summary>
        static DataCore()
        {

#if UNITY_IOS
        dataEncoding = Encoding.UTF8;
#else
            dataEncoding = Encoding.Default;
#endif

            dataCoderDirPath = Application.persistentDataPath + "/DC";
            if (!Directory.Exists(dataCoderDirPath))
            {
                Directory.CreateDirectory(dataCoderDirPath);
            }
            dataCoderDirPath += "/";

            allKeys = new HashSet<string>();
            //ReloadInternalKeys();
        }
        /// <summary>
        /// 主动静态初始化
        /// </summary>
        public static void Init() { }

        #endregion

        #region 方法

        #region 通用
        /// <summary>
        /// 获得Key文件路径
        /// </summary>
        private static string GetEncodeKeyPath(string key)
        {
            return dataCoderDirPath + key;
        }

        /// <summary>
        /// 删除键
        /// </summary>
        public static void RemoveKey(string key)
        {
            if (DeleteInternalKey(key))
            {
                File.Delete(GetEncodeKeyPath(key));
            }
        }

        /// <summary>
        /// 删除所有键
        /// </summary>
        public static void RemoveAllKeys()
        {
            allKeys.Clear();
            if (Directory.Exists(dataCoderDirPath))
            {
                Directory.Delete(dataCoderDirPath, true);
                Directory.CreateDirectory(dataCoderDirPath);
            }
        }

        /// <summary>
        /// 查找键
        /// </summary>
        public static bool HasKey(string key)
        {
            return allKeys.Contains(key);
        }

        /// <summary>
        /// 列举键
        /// </summary>
        public static HashSet<string> GetAllKeys()
        {
            return allKeys;
        }
        #endregion

        #region I/O文件操作
        /// <summary>
        /// 移除文件
        /// </summary>
        public static void RemoveFile(string filePath)
        {
            File.Delete(dataCoderDirPath + filePath);
        }

        /// <summary>
        /// 将字符串写入文件（不能使用绝对路径，相对于DataCore默认储存路径）
        /// </summary>
        /// <param name="str">待写入文本</param>
        /// <param name="fileRelativePath">文件相对路径</param>
        public static bool WriteStringToFile(string str, string fileRelativePath)
        {
            string path = dataCoderDirPath + fileRelativePath;

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
            }
            catch
            {
                if (writer != null)
                {
                    writer.Close();
                }
                return false;
            }

            writer.Write(str);
            writer.Close();
            return true;
        }

        /// <summary>
        /// 从文件读入字符串（不能使用绝对路径，相对于DataCore默认储存路径）
        /// </summary>
        /// <param name="fileRelativePath">文件相对路径</param>
        public static string GetStringFromFile(string fileRelativePath)
        {
            var path = dataCoderDirPath + fileRelativePath;
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            }
            catch
            {
                if (reader != null)
                {
                    reader.Close();
                }
                return null;
            }

            var ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        #region 保存类型数据
        /// <summary>
        /// 保存值或结构
        /// </summary>
        public static void SaveValue(string key, object value)
        {
            JsonData jsonData = value as JsonData;
            if (jsonData != null)
            {
                value = jsonData.ToJson();
            }
            SaveToEncodeBytes(key, GetValueBytes(value));
        }

        /// <summary>
        /// 保存列表
        /// </summary>
        /// <typeparam name="V">列表子类型</typeparam>
        /// <param name="key">要保存的键</param>
        /// <param name="value">要保存的列表</param>
        public static void SaveValue<V>(string key, List<V> value)
        {
            SaveList(key, value);
        }

        /// <summary>
        /// 保存设定集
        /// </summary>
        /// <typeparam name="V">设定集子类型</typeparam>
        /// <param name="key">要保存的键</param>
        /// <param name="value">要保存的设定集</param>
        public static void SaveValue<V>(string key, HashSet<V> value)
        {
            SaveHashSet(key, value);
        }

        /// <summary>
        /// 保存字典
        /// </summary>
        /// <param name="key">要保存的键</param>
        /// <param name="value">要保存的字典</param>
        public static void SaveValue<K, V>(string key, Dictionary<K, V> value)
        {
            SaveDict(key, value);
        }

        /// <summary>
        /// 保存顺序字典
        /// </summary>
        /// <param name="key">要保存的键</param>
        /// <param name="value">要保存的顺序字典</param>
        public static void SaveValue<K, V>(string key, SortedDictionary<K, V> value)
        {
            SaveSortedDict(key, value);
        }

        /// <summary>
        /// 读取值或结构（返回是否读取成功）
        /// </summary>
        public static bool LoadValue<T>(string key, out T outValue)
        {
            var readBytes = LoadFromEncodeBytes(key);
            if (readBytes == null)
            {
                outValue = default(T);
                return false;
            }

            var type = typeof(T);
            var typeCode = System.Type.GetTypeCode(type);

            if (type.IsArray)
            {
                typeCode = System.Type.GetTypeCode(type.GetElementType());
            }

            switch (typeCode)
            {
                case System.TypeCode.Int16:
                    if (type.IsArray)
                    {
                        int typeSize = 2;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new short[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToInt16(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToInt16(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.Int32:
                    if (type.IsArray)
                    {
                        int typeSize = 4;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new int[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToInt32(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToInt32(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.Int64:
                    if (type.IsArray)
                    {
                        int typeSize = 8;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new long[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToInt64(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToInt64(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.UInt16:
                    if (type.IsArray)
                    {
                        int typeSize = 2;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new ushort[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToUInt16(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToUInt16(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.UInt32:
                    if (type.IsArray)
                    {
                        int typeSize = 4;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new uint[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToUInt32(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToUInt32(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.UInt64:
                    if (type.IsArray)
                    {
                        int typeSize = 8;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new ulong[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToUInt64(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToUInt64(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.Byte:
                    if (type.IsArray)
                    {
                        outValue = (T)((object)readBytes);
                    }
                    else
                    {
                        outValue = (T)((object)readBytes[0]);
                    }
                    return true;
                case System.TypeCode.SByte:
                    if (type.IsArray)
                    {
                        int typeSize = 2;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new sbyte[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = (sbyte)System.BitConverter.ToInt16(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)(sbyte)System.BitConverter.ToInt16(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.Char:
                    if (type.IsArray)
                    {
                        int typeSize = 2;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new char[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToChar(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToChar(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.Single:
                    if (type.IsArray)
                    {
                        int typeSize = 4;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new float[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToSingle(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToSingle(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.Double:
                    if (type.IsArray)
                    {
                        int typeSize = 8;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new double[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToDouble(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToDouble(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.String:
                    if (type.IsArray) break;
                    outValue = (T)((object)dataEncoding.GetString(readBytes));
                    return true;
                case System.TypeCode.Boolean:
                    if (type.IsArray)
                    {
                        int typeSize = 1;
                        int readCount = readBytes.Length / typeSize;
                        var outValuePointer = new bool[readCount];
                        for (int i = 0; i < readCount; i++)
                        {
                            outValuePointer[i] = System.BitConverter.ToBoolean(readBytes, typeSize * i);
                        }
                        outValue = (T)(object)outValuePointer;
                    }
                    else
                    {
                        outValue = (T)((object)System.BitConverter.ToBoolean(readBytes, 0));
                    }
                    return true;
                case System.TypeCode.Decimal:
                    if (type.IsArray) break;
                    outValue = (T)((object)decimal.Parse(dataEncoding.GetString(readBytes)));
                    return true;
                case System.TypeCode.Object:
                    {
                        if (type.IsArray) break;

                        var value = dataEncoding.GetString(readBytes);
                        outValue = default(T);
                        if (type.FullName.StartsWith("LitJ"))
                        {
                            try
                            {
                                outValue = (T)(object)JsonMapper.ToObject(value);
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        else
                        {
                            try
                            {
                                outValue = (T)JsonUtility.FromJson(value, type);
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        return true;
                    }
            }

            outValue = default(T);
            return false;
        }

        /// <summary>
        /// 读取列表（返回是否读取成功）
        /// </summary>
        /// <typeparam name="V">列表子类型</typeparam>
        /// <param name="key">要读取的键</param>
        /// <param name="outValue">输出的列表</param>
        public static bool LoadValue<V>(string key, out List<V> outValue)
        {
            outValue = LoadList<V>(key);
            return outValue != null;
        }
        /// <summary>
        /// 读取设定集（返回是否读取成功）
        /// </summary>
        /// <typeparam name="V">设定集子类型</typeparam>
        /// <param name="key">要读取的键</param>
        /// <param name="outValue">输出的设定集</param>
        public static bool LoadValue<V>(string key, out HashSet<V> outValue)
        {
            outValue = LoadHashSet<V>(key);
            return outValue != null;
        }

        /// <summary>
        /// 读取字典（返回是否读取成功）
        /// </summary>
        /// <param name="key">要读取的键</param>
        /// <param name="outValue">输出的字典</param>
        public static bool LoadValue<K, V>(string key, out Dictionary<K, V> outValue)
        {
            outValue = LoadDict<K, V>(key);
            return outValue != null;
        }

        /// <summary>
        /// 读取顺序字典（返回是否读取成功）
        /// </summary>
        /// <param name="key">要读取的键</param>
        /// <param name="outValue">输出的顺序字典</param>
        public static bool LoadValue<K, V>(string key, out SortedDictionary<K, V> outValue)
        {
            outValue = LoadSortedDict<K, V>(key);
            return outValue != null;
        }
        #endregion

        #region 保存类型数据辅助方法
        /// <summary>
        /// 保存list
        /// </summary>
        public static void SaveList<T>(string key, List<T> value)
        {
            string json;
            try
            {
                json = JsonUtility.ToJson(new SerializationList<T>(value));
            }
            catch
            {
                return;
            }
            SaveToEncodeBytes(key, dataEncoding.GetBytes(json));
        }

        /// <summary>
        /// 保存设定集
        /// </summary>
        public static void SaveHashSet<T>(string key, HashSet<T> value)
        {
            string json;
            try
            {
                json = JsonUtility.ToJson(new SerializationHashSet<T>(value));
            }
            catch
            {
                return;
            }
            SaveToEncodeBytes(key, dataEncoding.GetBytes(json));
        }

        /// <summary>
        /// 保存字典
        /// </summary>
        public static void SaveDict<K, V>(string key, Dictionary<K, V> value)
        {
            string json;
            try
            {
                json = JsonUtility.ToJson(new SerializationDictionary<K, V>(value));
            }
            catch
            {
                return;
            }
            SaveToEncodeBytes(key, dataEncoding.GetBytes(json));
        }

        /// <summary>
        /// 保存顺序字典
        /// </summary>
        public static void SaveSortedDict<K, V>(string key, SortedDictionary<K, V> value)
        {
            string json;
            try
            {
                json = JsonUtility.ToJson(new SerializationSortedDictionary<K, V>(value));
            }
            catch
            {
                return;
            }
            SaveToEncodeBytes(key, dataEncoding.GetBytes(json));
        }

        /// <summary>
        /// 读取链表
        /// </summary>
        public static List<T> LoadList<T>(string key)
        {
            var readBytes = LoadFromEncodeBytes(key);
            if (readBytes == null) return null;

            var value = dataEncoding.GetString(readBytes);
            if (value == null) return null;

            SerializationList<T> afterValue = JsonMapper.ToObject<SerializationList<T>>(value);
            if (afterValue == null) return null;

            return afterValue.ToList();
        }

        /// <summary>
        /// 读取设定集
        /// </summary>
        public static HashSet<T> LoadHashSet<T>(string key)
        {
            var readBytes = LoadFromEncodeBytes(key);
            if (readBytes == null) return null;

            var value = dataEncoding.GetString(readBytes);
            if (value == null) return null;

            var afterValue = JsonMapper.ToObject<SerializationHashSet<T>>(value);
            if (afterValue == null) return null;

            return afterValue.ToHashSet();
        }

        /// <summary>
        /// 读取字典
        /// </summary>
        public static Dictionary<K, V> LoadDict<K, V>(string key)
        {
            var readBytes = LoadFromEncodeBytes(key);
            if (readBytes == null) return null;

            var value = dataEncoding.GetString(readBytes);
            if (value == null) return null;

            var afterValue = JsonMapper.ToObject<SerializationDictionary<K, V>>(value);
            if (afterValue == null) return null;

            return afterValue.ToDictionary();
        }

        /// <summary>
        /// 读取顺序字典
        /// </summary>
        public static SortedDictionary<K, V> LoadSortedDict<K, V>(string key)
        {
            var readBytes = LoadFromEncodeBytes(key);
            if (readBytes == null) return null;

            var value = dataEncoding.GetString(readBytes);
            if (value == null) return null;

            var afterValue = JsonMapper.ToObject<SerializationSortedDictionary<K, V>>(value);
            if (afterValue == null) return null;

            return afterValue.ToSortedDictionary();
        }

        /// <summary>
        /// 保存字节
        /// </summary>
        public static bool SaveToEncodeBytes(string key, byte[] bytes)
        {
            if (!enableSave) return false;

            if (!SaveToEncodeBytesInternal(key, bytes))
            {
                System.Threading.Thread.Sleep(50);
                return SaveToEncodeBytesInternal(key, bytes);
            }
            return true;
        }

        /// <summary>
        /// 保存字节(内部调用）
        /// </summary>
        private static bool SaveToEncodeBytesInternal(string key, byte[] bytes)
        {
            BinaryWriter writer = null;

            try
            {
                writer = new BinaryWriter(new FileStream(GetEncodeKeyPath(key), FileMode.Create, FileAccess.Write));
            }
            catch
            {
                if (writer != null)
                {
                    writer.Close();
                }
                return false;
            }
            AddInternalKey(key);
            var lenBytes = System.BitConverter.GetBytes(bytes.Length);

            try
            {
                writer.Write(lenBytes);
                for (int i = 0; i < bytes.Length; i++)
                {
                    writer.Write(bytes[i]);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                writer.Close();
            }
            return true;
        }

        /// <summary>
        /// 读取字节并解密
        /// </summary>
        public static byte[] LoadFromEncodeBytes(string key)
        {
            byte[] ret = null;

            do
            {
                BinaryReader reader = null;

                int lenBytes;

                try
                {
                    reader = new BinaryReader(new FileStream(GetEncodeKeyPath(key), FileMode.Open, FileAccess.Read));
                    lenBytes = reader.ReadInt32();

                    if (lenBytes <= 0)
                    {
                        reader.Close();
                        break;
                    }

                    ret = new byte[lenBytes];
                    for (int i = 0; i < lenBytes; i++)
                    {
                        ret[i] = reader.ReadByte();
                    }
                    reader.Close();
                }
                catch
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    break;
                }
            } while (false);

            return ret;
        }

        /// <summary>
        /// 获取value值的字节码
        /// </summary>
        public static byte[] GetValueBytes(object value)
        {
            if (value == null)
            {
                return new byte[0];
            }

            Type type = value.GetType();
            TypeCode typeCode = Type.GetTypeCode(type);//类型枚举

            if (type.IsArray)
            {
                var eleType = type.GetElementType();
                var eleTypeCode = System.Type.GetTypeCode(eleType);
                if (eleType.IsClass || eleTypeCode == System.TypeCode.Decimal)
                {
                    throw new System.Exception("Can not get value bytes from " + value);
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    var tArray = (System.Array)value;
                    var tArrayLen = tArray.Length;

                    for (int ti = 0; ti < tArrayLen; ti++)
                    {
                        var tBytes = GetValueBytes(tArray.GetValue(ti));
                        memoryStream.Write(tBytes, 0, tBytes.Length);
                    }
                    return memoryStream.ToArray();
                }
            }

            switch (typeCode)
            {
                case TypeCode.Int16:
                    return BitConverter.GetBytes((short)value);
                case TypeCode.Int32:
                    return BitConverter.GetBytes((int)value);
                case TypeCode.Int64:
                    return BitConverter.GetBytes((long)value);
                case TypeCode.UInt16:
                    return BitConverter.GetBytes((ushort)value);
                case TypeCode.UInt32:
                    return BitConverter.GetBytes((uint)value);
                case TypeCode.UInt64:
                    return BitConverter.GetBytes((ulong)value);
                case TypeCode.Byte:
                    return new byte[] { (byte)value };
                case TypeCode.SByte:
                    return BitConverter.GetBytes((short)value);
                case TypeCode.Char:
                    return BitConverter.GetBytes((char)value);
                case TypeCode.Single:
                    return BitConverter.GetBytes((float)value);
                case TypeCode.Double:
                    return BitConverter.GetBytes((double)value);
                case TypeCode.String:
                    return dataEncoding.GetBytes((string)value);
                case TypeCode.Boolean:
                    return BitConverter.GetBytes((bool)value);
                case TypeCode.Decimal:
                    return dataEncoding.GetBytes(value.ToString());
                case TypeCode.Object:
                    {
                        string json;
                        try
                        {
                            json = JsonUtility.ToJson(value);
                        }
                        catch
                        {
                            throw new System.Exception("Can not get value bytes from " + value);
                        }
                        return dataEncoding.GetBytes(json);
                    }
            }

            throw new System.Exception("Can not get value bytes from " + value);
        }
        #endregion

        #endregion

        #region Unity文件管理
        /// <summary>
        /// 从Resources(资源管理包)获得Json对象
        /// </summary>
        public static JsonData GetJsonFromAssetsResources(string filePathWithNoPostfix)
        {
            JsonData ret;
            TextAsset resObj;
            try
            {
                resObj = Resources.Load<TextAsset>(filePathWithNoPostfix);
            }
            catch
            {
                return null;
            }

            if (resObj == null) return null;

            try
            {
                string str = resObj.text;
                ret = JsonMapper.ToObject(str);
            }
            catch
            {
                return null;
            }
            finally
            {
                Resources.UnloadAsset(resObj);
            }

            return ret;
        }

        #endregion

        #region 数据读取相关
        /// <summary>
        /// 从json字符串对象组加载到自定义类型键结构体字典组
        /// </summary>
        /// <typeparam name="K">自定义键（一般是整型或者字符串）</typeparam>
        /// <typeparam name="V">信息结构体类型（一般是静态信息）</typeparam>
        /// <param name="objDict">需要被加载的字典</param>
        /// <param name="jsonData">json对象</param>
        /// <param name="onLoadProgressCallback">当每次加载和加载完成时回调，当你在回调中返回true，表示你已经手动处理该次赋值，不再自动赋值</param>
        public static void ReloadDictFromJsonDataGroup<K, V>(Dictionary<K, V> objDict, JsonData jsonData, System.Predicate<FieldLoadProgressInfo> onLoadProgressCallback = null)
            where V : new()
        {
            objDict.Clear();

            V obj;
            var type = typeof(V);

            BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var allFields = type.GetFields(flag).Where(f => f.IsPublic || f.HasAttribute<SerializeField>());

            var fieldLoadProgressInfo = new FieldLoadProgressInfo();
            ToleranceJsonData avm = new ToleranceJsonData();
            ToleranceJsonData subAvm = new ToleranceJsonData();
            System.TypeCode keyTypeCode = System.Type.GetTypeCode(typeof(K));
            bool isStrKey = (keyTypeCode == System.TypeCode.String || keyTypeCode == System.TypeCode.Object);

            foreach (KeyValuePair<string, JsonData> subItem in jsonData)
            {
                obj = new V();
                objDict.Add(subItem.Key.ParseTo<K>(), obj);

                avm = subItem.Value;

                foreach (var fieldInfo in allFields)
                {
                    subAvm = avm[fieldInfo.Name];
                    if (subAvm.HasError) continue;

                    fieldLoadProgressInfo.ObjectInstance = obj;
                    fieldLoadProgressInfo.IsFinished = false;
                    fieldLoadProgressInfo.NowFieldInfo = fieldInfo;
                    fieldLoadProgressInfo.TableKey = fieldInfo.Name;
                    fieldLoadProgressInfo.TableValue = subAvm.AsString();

                    if (onLoadProgressCallback == null || !onLoadProgressCallback(fieldLoadProgressInfo))
                    {
                        fieldLoadProgressInfo.AutoSetTableValueToFieldInfo(isStrKey);
                    }
                }

                onLoadProgressCallback?.Invoke(new FieldLoadProgressInfo() { ObjectInstance = obj, IsFinished = true });
            }
        }

        /// <summary>
        /// 从json字符串对象组加载到整型键结构体字典组
        /// </summary>
        /// <typeparam name="T">信息结构体类型（一般是静态信息）</typeparam>
        /// <param name="objDict">需要被加载的字典</param>
        /// <param name="jsonData">json对象</param>
        /// <param name="onLoadProgressCallback">当每次加载和加载完成时回调，当你在回调中返回true，表示你已经手动处理该次赋值，不再自动赋值</param>
        public static void ReloadIntDictFromJsonDataGroup<T>(Dictionary<int, T> objDict, JsonData jsonData, System.Predicate<FieldLoadProgressInfo> onLoadProgressCallback = null)
            where T : new()
        {
            ReloadDictFromJsonDataGroup(objDict, jsonData, onLoadProgressCallback);
        }

        /// <summary>
        /// 从 Resources json 文件读取到整型键结构体字典组
        /// </summary>
        /// <typeparam name="T">信息结构体类型（一般是静态信息）</typeparam>
        /// <param name="filePathWithNoSuffix"></param>
        /// <param name="onLoadProgressCallback">当每次加载和加载完成时回调，当你在回调中返回true，表示你已经手动处理该次赋值，不再自动赋值</param>
        public static Dictionary<int, T> GetIntDictFromAssetsResourcesJson<T>(string filePathWithNoSuffix, System.Predicate<FieldLoadProgressInfo> onLoadProgressCallback = null)
            where T : new()
        {
            Dictionary<int, T> ret;

            JsonData jsonData = GetJsonFromAssetsResources(filePathWithNoSuffix);

            if (jsonData == null) return null;

            ret = new Dictionary<int, T>();
            ReloadIntDictFromJsonDataGroup(ret, jsonData, onLoadProgressCallback);

            return ret;
        }

        /// <summary>
        /// 从json字符串对象组加载到字符键结构体字典组
        /// </summary>
        /// <typeparam name="T">信息结构体类型（一般是静态信息）</typeparam>
        /// <param name="objDict">需要被加载的字典</param>
        /// <param name="jsonData">json对象</param>
        /// <param name="onLoadProgressCallback">当每次加载和加载完成时回调，当你在回调中返回true，表示你已经手动处理该次赋值，不再自动赋值</param>
        public static void ReloadStringDictFromJsonDataGroup<T>(Dictionary<string, T> objDict, JsonData jsonData, System.Predicate<FieldLoadProgressInfo> onLoadProgressCallback = null)
            where T : new()
        {
            ReloadDictFromJsonDataGroup(objDict, jsonData, onLoadProgressCallback);
        }


        /// <summary>
        /// 从字符串返回相应类型值
        /// </summary>
        public static object ConvertStringToUniversalValue(string value, System.Type type, bool noLogError = false)
        {
            bool isEmpty = (value.Length == 0 || value.Equals("-"));

            var typeCode = System.Type.GetTypeCode(type);

            try
            {
                switch (typeCode)
                {
                    case System.TypeCode.Byte:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return byte.Parse(value);
                    case System.TypeCode.SByte:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return sbyte.Parse(value);
                    case System.TypeCode.Char:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return char.Parse(value);
                    case System.TypeCode.Int16:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return short.Parse(value);
                    case System.TypeCode.Int32:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return int.Parse(value);
                    case System.TypeCode.Int64:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return long.Parse(value);
                    case System.TypeCode.UInt16:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return ushort.Parse(value);
                    case System.TypeCode.UInt32:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return uint.Parse(value);
                    case System.TypeCode.UInt64:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return ulong.Parse(value);
                    case System.TypeCode.Decimal:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return decimal.Parse(value);
                    case System.TypeCode.Single:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return float.Parse(value);
                    case System.TypeCode.Double:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                        return double.Parse(value);
                    case System.TypeCode.String:
                        return isEmpty ? "" : value;
                    case System.TypeCode.Boolean:
                        if (isEmpty) throw new System.Exception("DataCoder: Can not convert your " + type);
                        if (value == "0" || value == "false") return false;
                        return true;
                    case System.TypeCode.Object:
                        {
                            if (!type.IsClass && !type.IsArray) break;
                            if (isEmpty)
                            {
                                if (type.IsArray)
                                {
                                    return System.Activator.CreateInstance(type, 0);
                                }

                                if (type.ToString().Contains("List`"))
                                {
                                    return System.Activator.CreateInstance(type);
                                }

                                throw new System.Exception("DataCoder: Can not convert your " + type + ", it is empty!");
                            }

                            if (type.IsArray)
                            {
                                return ConvertStringGroupToArray(value, type);
                            }

                            var listSubType = GetListSubType(type);
                            if (listSubType != null)
                            {
                                return ConvertStringGroupToList(value, type);
                            }
                            else
                            {
                                return ConvertStringGroupToStruct(value, type);
                            }
                        }
                }
                throw new System.Exception("DataCoder: Can not convert your " + type + ", not support this type!");
            }
            catch (System.Exception e)
            {
                if (!noLogError) Debug.LogError($"DataCoder: Can not convert your \"{value}\" to {type}!");
                throw e;
            }
        }

        /// <summary>
        /// 从单/多集合字符串转换到数组
        /// </summary>
        private static object ConvertStringGroupToArray(string value, System.Type type)
        {
            if (!type.IsArray) return null;

            char splitChar = '|';

            byte containCharCount = 0;

            if (value.Contains('|'))
            {
                containCharCount++;
            }

            if (value.Contains(';'))
            {
                splitChar = ';';
                containCharCount++;
            }

            string[] strs;
            int len;

            //当只有单一元素时，应判断该集合是单集合还是多集合
            if (containCharCount < 2 && type.GetElementType().IsClass && System.Type.GetTypeCode(type.GetElementType()) != System.TypeCode.String)
            {
                strs = new string[] { value };
                len = 1;
            }
            else
            {
                strs = value.Split(splitChar);
                len = strs.Length;
            }

            var arrayObj = (System.Array)System.Activator.CreateInstance(type, len);

            if (len > 0)
            {
                var eType = type.GetElementType();

                for (int i = 0; i < len; i++)
                {
                    arrayObj.SetValue(ConvertStringToUniversalValue(strs[i], eType), i);
                }
            }

            return arrayObj;
        }

        /// <summary>
        /// 获得List的子物体类型
        /// </summary>
        /// <param name="listType">List的类型实例</param>
        public static System.Type GetListSubType(System.Type listType)
        {
            string underlyingTypeName = listType.ToString();

            if (!underlyingTypeName.Contains("List`")) return null;

            int firstIndex = underlyingTypeName.IndexOf('[');
            int lastIndex = underlyingTypeName.LastIndexOf(']');

            string subTypeName = underlyingTypeName.Substring(firstIndex + 1, lastIndex - firstIndex - 1);
            return System.Type.GetType(subTypeName);
        }

        /// <summary>
        /// 从单集合字符串转换到List
        /// </summary>
        private static object ConvertStringGroupToList(string value, System.Type listType)
        {
            var subType = GetListSubType(listType);
            if (subType == null) return null;

            char splitChar = '|';

            byte containCharCount = 0;

            if (value.Contains('|'))
            {
                containCharCount++;
            }

            if (value.Contains(';'))
            {
                splitChar = ';';
                containCharCount++;
            }

            string[] strs;
            int len;

            //当只有单一元素时，应判断该集合是单集合还是多集合
            if (containCharCount < 2 && subType.IsClass && System.Type.GetTypeCode(subType) != System.TypeCode.String)
            {
                strs = new string[] { value };
                len = 1;
            }
            else
            {
                strs = value.Split(splitChar);
                len = strs.Length;
            }

            var arrayObj = (System.Collections.IList)System.Activator.CreateInstance(listType);

            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    arrayObj.Add(ConvertStringToUniversalValue(strs[i], subType));
                }
            }

            return arrayObj;
        }

        /// <summary>
        /// 从单集合字符串转换到结构体
        /// </summary>
        private static object ConvertStringGroupToStruct(string value, System.Type type)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fieldInfos = type.GetFields(flag).Where(f => f.GetOrdinal() != -1).ToList();
            fieldInfos.Sort((a, b) =>
            {
                int oa = a.GetOrdinal();
                int ob = b.GetOrdinal();
                return oa < ob ? -1 : (oa == ob ? 0 : 1);
            });

            char splitChar = '|';
            if (value.Contains(';')) splitChar = ';';
            var strs = value.Split(splitChar);
            if (fieldInfos.Count != strs.Length)
            {
                throw new System.Exception("DataCoder: Can not convert " + value + " to " + type + " because the field count is not equal!");
            }
            //int minLength = fieldInfos.Count < strs.Length ? fieldInfos.Count : strs.Length;
            //if (minLength < 1) return null;

            var obj = System.Activator.CreateInstance(type);

            for (int i = 0; i < strs.Length; i++)
            {
                fieldInfos[i].SetValue(obj, ConvertStringToUniversalValue(strs[i], fieldInfos[i].FieldType));
            }

            return obj;
        }

        #endregion

        #endregion
    }
}