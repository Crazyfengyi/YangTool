/*
 *Copyright(C) 2020 by Yang
 *All rights reserved.
 *脚本功能:     #FUNCTION#
 *Author:       陈春洋
 *UnityVersion：2019.3.3f1
 *创建时间:         2020-08-27
*/

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace YangTools.Scripts.Core.YangDataCode
{
    /// <summary>
    /// 自动保存类(用的ES3.Save)
    /// </summary>
    /// <typeparam name="T">要保存的类型</typeparam>
    public class AutoSave<T>
    {
        private bool isLoad;
        /// <summary>
        /// 自动保存用的Key
        /// </summary>
        public string SaveKey { get; }
        /// <summary>
        /// 是否开启自动保存--重复连续更改Value请暂时关闭
        /// </summary>
        public bool IsOpenAutoSave { get; }

        private T value;

        public T Value
        {
            get
            {
                if (isLoad == false)
                {
                    Load();
                }
                return value;
            }
            set
            {
                this.value = value;
                if (IsOpenAutoSave)
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// 默认使用的key:调用方的方法名+文件路径
        /// </summary>
        /// <param name="saveKey">保存用的Key</param>
        /// <param name="callName">key1:默认为调用方的方法名/字段名</param>
        /// <param name="path">key2:默认为调用方文件路径名</param>
        public AutoSave(string saveKey = "", [CallerMemberName] string callName = "", [CallerFilePath] string path = "")
        {
            if (string.IsNullOrEmpty(saveKey))
            {
                SaveKey = callName + "_" + path.GetHashCode();
            }
            else
            {
                SaveKey = saveKey;
            }

            IsOpenAutoSave = true;
        }

        public void Save()
        {
            ES3.Save<T>(SaveKey, value);
        }

        public void Load()
        {
            isLoad = true;
            if (ES3.KeyExists(SaveKey))
            {
                T loadValue = ES3.Load<T>(SaveKey);
                value = loadValue;
            }
        }
    }

    /// <summary>
    /// 数据处理
    /// </summary>
    public static class YangDataCode
    {
        /// <summary>
        /// 编码分隔符
        /// </summary>
        private const string EncodeSeparator = ".";
        /// <summary>
        /// string 转 byte string  中文转byte存储
        /// </summary>
        private static string EncodeStringToByte(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return string.Join(EncodeSeparator, Encoding.UTF8.GetBytes(str));
        }
        /// <summary>
        /// byte string 转 string
        /// </summary>
        public static string DecodeByteToString(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            string[] encodedArray = str.Split(new string[] { EncodeSeparator }, StringSplitOptions.RemoveEmptyEntries);
            return Encoding.UTF8.GetString(Array.ConvertAll(encodedArray, s => byte.Parse(s)));
        }
        
        // encodeChinese ()  bytes[]  .join(".")  "111.255.133.132.321.3.545.656.656.67.434"
        // decode  "111.255.133.132.321.3.545.656.656.67.434" -> split()   ["111","255"]  => byte.pare()=> byte[]  => convert=>  string 
    }
}