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

        private const string PlayerLocalSaveDataKey = "PlayerLocalSaveData";

        public void OnEnable()
        {
            string saveData = PlayerPrefs.GetString(PlayerLocalSaveDataKey);
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
                PlayerPrefs.SetString(PlayerLocalSaveDataKey, saveData);
                PlayerPrefs.Save();

#if UNITY_EDITOR
                string saveLocalFile = $"{Application.persistentDataPath}/{PlayerLocalSaveDataKey}";
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
        public T GetLocalSave<T>(bool isDirty = false) where T : SaveDataBase, new()
        {
            var saveTypeKey = typeof(T).Name;
            if (isDirty) DirtyKey.Add(saveTypeKey);

            foreach (var item in localSaves)
            {
                if (item.saveKey.Equals(saveTypeKey))
                {
                    if (item.SaveDataBase == null)
                    {
                        item.Deserialize<T>();
                    }

                    if (item.SaveDataBase is T t)
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
        [NonSerialized] public SaveDataBase SaveDataBase;

        public LocalSaveData(string saveName, SaveDataBase saveDataBase)
        {
            saveKey = saveName;
            SaveDataBase = saveDataBase;
        }

        public void Serialize()
        {
            if (SaveDataBase != null) saveJson = JsonUtility.ToJson(SaveDataBase);
        }

        public void Deserialize<T>() where T : SaveDataBase, new()
        {
            if (!string.IsNullOrEmpty(saveJson))
            {
                SaveDataBase = JsonUtility.FromJson<T>(saveJson);
                SaveDataBase.OnAfterDeserialize();
            }
        }
    }

    [Serializable]
    public abstract class SaveDataBase
    {
        public abstract void SetDefaultData(string tableData);

        public virtual void OnAfterDeserialize()
        {
        }
    }

    /// <summary>
    /// 游戏设置
    /// </summary>
    public class SaveGameSet : SaveDataBase
    {
        /// <summary>
        /// 音乐开关
        /// </summary>
        public bool IsOnMusic;

        public override void SetDefaultData(string tableData)
        {
        }
    }

    /// <summary>
    /// 游戏信息存储
    /// </summary>
    public class SaveGameDataBase : SaveDataBase
    {
        /// <summary>
        /// 是否首次进入
        /// </summary>
        public bool IsFirstEnter;

        public override void SetDefaultData(string tableData)
        {
            IsFirstEnter = true;
        }
    }
    
    /// <summary>
    /// 任务系统存档。
    /// </summary>
    public class Save_QuestData : SaveDataBase
    {
        public List<SaveQuestItem> quests;

        public override void SetDefaultData(string tableData)
        {
            quests = new List<SaveQuestItem>();
        }

        public override void OnAfterDeserialize()
        {
            quests ??= new List<SaveQuestItem>();
            for (int i = 0; i < quests.Count; i++)
            {
                quests[i]?.OnAfterDeserialize();
            }
        }

        /// <summary>
        /// 获取指定任务存档
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>任务存档，不存在时返回null</returns>
        public SaveQuestItem GetQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) || quests == null)
            {
                return null;
            }

            for (int i = 0; i < quests.Count; i++)
            {
                SaveQuestItem item = quests[i];
                if (item != null && string.Equals(item.questId, questId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取或创建任务存档。
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>任务存档</returns>
        public SaveQuestItem GetOrCreateQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                throw new ArgumentException("任务ID不能为空", nameof(questId));
            }

            quests ??= new List<SaveQuestItem>();
            SaveQuestItem existingItem = GetQuest(questId);
            if (existingItem != null)
            {
                return existingItem;
            }

            SaveQuestItem item = new SaveQuestItem
            {
                questId = questId,
                state = QuestState.Locked,
                objectives = new List<SaveQuestObjectiveItem>()
            };
            quests.Add(item);
            return item;
        }
    }
    
     /// <summary>
    /// 单个任务存档。
    /// </summary>
    [Serializable]
    public class SaveQuestItem
    {
        public string questId;
        public string dailyRefreshDate;
        public QuestState state;
        public List<SaveQuestObjectiveItem> objectives;

        public void OnAfterDeserialize()
        {
            dailyRefreshDate ??= string.Empty;
            objectives ??= new List<SaveQuestObjectiveItem>();
            for (int i = 0; i < objectives.Count; i++)
            {
                objectives[i]?.OnAfterDeserialize();
            }
        }

        /// <summary>
        /// 获取指定目标存档。
        /// </summary>
        /// <param name="objectiveId">目标ID</param>
        /// <returns>目标存档</returns>
        public SaveQuestObjectiveItem GetObjective(string objectiveId)
        {
            objectives ??= new List<SaveQuestObjectiveItem>();
            for (int i = 0; i < objectives.Count; i++)
            {
                SaveQuestObjectiveItem item = objectives[i];
                if (item != null && string.Equals(item.objectiveId, objectiveId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 按目标列表索引获取存档数据
        /// </summary>
        /// <param name="index">目标索引</param>
        /// <returns>目标存档</returns>
        public SaveQuestObjectiveItem GetObjectiveAt(int index)
        {
            if (index < 0 || objectives == null || index >= objectives.Count)
            {
                return null;
            }

            return objectives[index];
        }
    }

    /// <summary>
    /// 单个任务目标存档。
    /// </summary>
    [Serializable]
    public class SaveQuestObjectiveItem
    {
        public string objectiveId;
        public bool isCompleted;
        public List<SaveQuestConditionItem> conditions;

        public void OnAfterDeserialize()
        {
            conditions ??= new List<SaveQuestConditionItem>();
            for (int i = 0; i < conditions.Count; i++)
            {
                conditions[i]?.OnAfterDeserialize();
            }
        }

        /// <summary>
        /// 获取指定条件存档。
        /// </summary>
        /// <param name="conditionId">条件ID</param>
        /// <returns>条件存档</returns>
        public SaveQuestConditionItem GetCondition(string conditionId)
        {
            conditions ??= new List<SaveQuestConditionItem>();
            for (int i = 0; i < conditions.Count; i++)
            {
                SaveQuestConditionItem item = conditions[i];
                if (item != null && string.Equals(item.conditionId, conditionId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 按条件列表索引获取存档数据
        /// </summary>
        /// <param name="index">条件索引</param>
        /// <returns>条件存档</returns>
        public SaveQuestConditionItem GetConditionAt(int index)
        {
            if (index < 0 || conditions == null || index >= conditions.Count)
            {
                return null;
            }

            return conditions[index];
        }
    }

    /// <summary>
    /// 单个任务条件存档。
    /// </summary>
    [Serializable]
    public class SaveQuestConditionItem
    {
        public string conditionId; //运行时内部条件键 不需要在任务配置中填写
        public float currentCount; //当前条件进度
        public long startUtcSeconds; //时间条件开始时间
        // 历史字段名保留兼容 实际存储单位为分钟
        public float onlineTimeSeconds; //累计在线时长分钟数

        /// <summary>
        /// 修正反序列化后的非法数据
        /// </summary>
        public void OnAfterDeserialize()
        {
            conditionId ??= string.Empty;
            currentCount = NormalizeNonNegative(currentCount);
            startUtcSeconds = Math.Max(0L, startUtcSeconds);
            onlineTimeSeconds = NormalizeNonNegative(onlineTimeSeconds);
        }

        /// <summary>
        /// 将非法数值转换为非负数
        /// </summary>
        /// <param name="value">待转换数值</param>
        /// <returns>有效的非负数</returns>
        private static float NormalizeNonNegative(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Math.Max(0f, value);
        }
    }
}
