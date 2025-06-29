/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:DESKTOP-JVG8VG4
 *UnityVersion：6000.0.17f1c1
 *创建时间:2025-05-26
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core;

namespace YangTools.Scripts.Core.YangSaveData
{
    public class YangSaveDataManager : MonoSingleton<YangSaveDataManager>
    {
        private DataCenter dataCenter;
        public DataCenter DataCenter => dataCenter;

        private const string PLAYER_LOCAL_SAVE_DATA_KEY = "PlayerLocalSaveData";

        public void OnEnable()
        {
            string saveData = PlayerPrefs.GetString(PLAYER_LOCAL_SAVE_DATA_KEY);
            if (string.IsNullOrEmpty(saveData) == false)
            {
                dataCenter = JsonUtility.FromJson<DataCenter>(saveData);
                dataCenter.LoadLocalDataed();
                Debug.Log($"加载玩家本地数据:{saveData}");
            }
            else
            {
                dataCenter = new DataCenter();
                dataCenter.Initialize();
                Debug.Log($"创建玩家本地数据");
            }
        }

        protected override void OnDestroy()
        {
            SaveLocalData(true);
        }

        private readonly float intervalTime = 15f;
        private float time;

        public void Update()
        {
            time += Time.unscaledDeltaTime;
            if (time >= intervalTime)
            {
                time = 0;
                if (dataCenter != null)
                {
                    SaveLocalData(true);
                }
            }

            if (dataCenter != null && dataCenter.DirtyKey.Count > 0)
            {
                SaveLocalData(false);
            }
        }

        /// <summary>
        /// 保存本地数据 
        /// </summary>
        public void SaveLocalData(bool force)
        {
            if (dataCenter != null)
            {
                dataCenter.SaveDirtyData(force);
                string saveData = JsonUtility.ToJson(dataCenter, true);
                PlayerPrefs.SetString(PLAYER_LOCAL_SAVE_DATA_KEY, saveData);
                PlayerPrefs.Save();

#if UNITY_EDITOR
                string saveLocalFile = $"{Application.persistentDataPath}/{PLAYER_LOCAL_SAVE_DATA_KEY}";
                if (!Directory.Exists(saveLocalFile))
                {
                    Directory.CreateDirectory(saveLocalFile);
                }

                string filePath = $"{saveLocalFile}/PLAYER_LOCAL_SAVE_DATA.json";
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                File.WriteAllText(filePath, saveData);
#endif
            }
        }

        /// <summary>
        /// 清空存档
        /// </summary>
        public async UniTask ClearSaveData()
        {
            dataCenter = new DataCenter();
            dataCenter.Initialize();
            SaveLocalData(true);
        }
    }

    [Serializable]
    public class DataCenter
    {
        //分块保存数据
        public List<LocalSaveData> localSaves;
        public HashSet<string> DirtyKey;

        public DataCenter()
        {
        }

        public void Initialize()
        {
            localSaves = new();
            DirtyKey = new();
        }

        /// <summary>
        /// 加载本地数据结束
        /// </summary>
        public void LoadLocalDataed()
        {
            DirtyKey = new();
        }

        /// <summary>
        /// 获得本地数据
        /// </summary>
        public T GetLocalSave<T>(bool isDirty = false) where T : ISaveData, new()
        {
            var saveTypeKey = typeof(T).Name;
            if (isDirty) DirtyKey.Add(saveTypeKey);

            foreach (var item in localSaves)
            {
                if (item.saveKey.Equals(saveTypeKey))
                {
                    if (item.saveData == null)
                    {
                        item.Deserialize<T>();
                    }

                    if (item.saveData is T t)
                    {
                        return t;
                    }

                    Debug.LogError($"数据错误:{saveTypeKey} = null");
                    break;
                }
            }

            var newSave = new T();
            //TODO:传全局表数据
            newSave.SetDefaultData("");
            var newCell = new LocalSaveData(saveTypeKey, newSave);
            localSaves.Add(newCell);
            Debug.Log($"添加本地数据:{newCell.saveKey}");
            return newSave;
        }

        /// <summary>
        /// 设置脏标记
        /// </summary>
        public void SaveDirtyData(bool force)
        {
            if (force)
            {
                foreach (var localSave in localSaves)
                {
                    localSave.Serialize();
                }
            }
            else if (DirtyKey.Count > 0)
            {
                foreach (var dirtyKey in DirtyKey)
                {
                    foreach (var localSave in localSaves)
                    {
                        if (localSave.saveKey.Equals(dirtyKey))
                        {
                            localSave.Serialize();
                            break;
                        }
                    }
                }

                DirtyKey.Clear();
            }
        }
    }

    [Serializable]
    public class LocalSaveData
    {
        public string saveKey;
        public string saveJson;
        [NonSerialized] public ISaveData saveData;

        public LocalSaveData(string saveName, ISaveData _saveData)
        {
            saveKey = saveName;
            saveData = _saveData;
        }

        public void Serialize()
        {
            if (saveData != null) saveJson = JsonUtility.ToJson(saveData);
        }

        public void Deserialize<T>() where T : ISaveData, new()
        {
            if (!string.IsNullOrEmpty(saveJson))
            {
                saveData = JsonUtility.FromJson<T>(saveJson);
                saveData.OnAfterDeserialize();
            }
        }
    }

    [Serializable]
    public abstract class ISaveData
    {
        public abstract void SetDefaultData(string tableData);

        public virtual void OnAfterDeserialize()
        {
        }
    }

    /// <summary>
    /// 游戏设置
    /// </summary>
    public class Save_GameSet : ISaveData
    {
        /// <summary>
        /// 音乐开关
        /// </summary>
        public bool isOnMusic;
        
        public override void SetDefaultData(string tableData)
        {
            
        }
    }
    
    /// <summary>
    /// 游戏信息存储
    /// </summary>
    public class Save_GameData : ISaveData
    {
        /// <summary>
        /// 是否首次进入
        /// </summary>
        public bool isFirstEnter;
        
        public override void SetDefaultData(string tableData)
        {
            isFirstEnter = true;
        }
    }
}