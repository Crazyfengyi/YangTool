/** 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION# 
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-08-27 
*/
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
        /// <summary>
        /// 自动保存用的Key
        /// </summary>
        public string SaveKey => saveKey;
        private bool isOpenAutoSave;
        /// <summary>
        /// 是否开启自动保存--重复连续更改Value请暂时关闭
        /// </summary>
        public bool IsOpenAutoSave => isOpenAutoSave;
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
        /// <param name="callName">key1:默认为调用方的方法名/字段名</param>
        /// <param name="path">key2:默认为调用方文件路径名</param>
        public AutoSave(string _saveKey, [CallerMemberName] string callName = "", [CallerFilePath] string path = "")
        {
            if (string.IsNullOrEmpty(_saveKey))
            {
                saveKey = callName + "_" + path.GetHashCode();
            }
            else
            {
                saveKey = _saveKey;
            }
            isOpenAutoSave = true;
        }
        public void Save()
        {
            ES3.Save<T>(saveKey, value);
        }
        public void Load()
        {
            isLoad = true;
            if (ES3.KeyExists(saveKey))
            {
                T loadValue = ES3.Load<T>(saveKey);
                value = loadValue;
            }
        }
    }
}