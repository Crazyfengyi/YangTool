/** 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION# 
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-08-27 
*/
using System;
using System.Runtime.CompilerServices;

namespace YangTools
{
    /// <summary>
    /// 自动保存类(用的ES3.Save)
    /// </summary>
    /// <typeparam name="T">要保存的类型</typeparam>
    public class AutoSave<T>
    {
        private bool isLoad;
        private string saveKey;
        public string SaveKey { get { return saveKey; } }
        private T _value;
        public T Value
        {
            get
            {
                if (isLoad == false)
                {
                    Load();
                }
                return _value;
            }
            set
            {
                _value = value;
                Save();
            }
        }
        /// <summary>
        /// 自定义key
        /// </summary>
        /// <param name="yourKey">你的key</param>
        public AutoSave(string yourKey)
        {
            saveKey = yourKey;
        }
        /// <summary>
        /// 默认使用的key:调用方的方法名+文件路径
        /// </summary>
        /// <param name="callName">key1:默认为调用方的方法名/字段名</param>
        /// <param name="path">key2:默认为调用方文件路径名</param>
        public AutoSave([CallerMemberName] string callName = "", [CallerFilePath] string path = "")
        {
            saveKey = callName + "_" + path.GetHashCode();
        }
        public void Save()
        {
            ES3.Save<T>(saveKey, _value);
        }
        public void Load()
        {
            isLoad = true;
            if (ES3.KeyExists(saveKey))
            {
                T loadValue = ES3.Load<T>(saveKey);
                _value = loadValue;
            }
        }
    }
}