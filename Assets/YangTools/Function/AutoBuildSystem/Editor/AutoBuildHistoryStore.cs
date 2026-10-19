#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 自动构建结果状态
    /// </summary>
    public enum AutoBuildResultStatus
    {
        Succeeded,
        Failed,
        Skipped
    }

    /// <summary>
    /// 一次自动构建的历史记录
    /// </summary>
    [Serializable]
    public sealed class AutoBuildBuildRecord
    {
        public string runId;
        public string profileName;
        public string startedAt;
        public string finishedAt;
        public double durationSeconds;
        public string status;
        public string message;
        public string logPath;
        public List<string> outputPaths = new List<string>();
    }

    /// <summary>
    /// 将构建历史保存在项目对应的用户本机目录
    /// </summary>
    internal static class AutoBuildHistoryStore
    {
        private const int MaximumHistoryCount = 50;

        [Serializable]
        private sealed class HistoryDocument
        {
            public List<AutoBuildBuildRecord> records = new List<AutoBuildBuildRecord>();
        }

        /// <summary>
        /// 追加一条构建记录并限制历史条数
        /// </summary>
        public static void Add(AutoBuildBuildRecord record)
        {
            AddToPath(AutoBuildLocalPaths.GetHistoryFilePath(), record);
        }

        /// <summary>
        /// 追加记录到指定 JSON 文件
        /// </summary>
        internal static void AddToPath(string path, AutoBuildBuildRecord record)
        {
            HistoryDocument history = Load(path);
            history.records.Insert(0, record);
            if (history.records.Count > MaximumHistoryCount)
            {
                history.records.RemoveRange(MaximumHistoryCount, history.records.Count - MaximumHistoryCount);
            }

            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(history));
            if (File.Exists(path)) File.Replace(temporaryPath, path, null);
            else File.Move(temporaryPath, path);
        }

        /// <summary>
        /// 读取最近的构建记录
        /// </summary>
        public static List<AutoBuildBuildRecord> LoadRecent(int count)
        {
            HistoryDocument history = Load(AutoBuildLocalPaths.GetHistoryFilePath());
            if (history.records.Count > count) history.records.RemoveRange(count, history.records.Count - count);
            return history.records;
        }

        /// <summary>
        /// 读取指定 JSON 文件中的记录
        /// </summary>
        internal static List<AutoBuildBuildRecord> LoadFromPath(string path)
        {
            return Load(path).records;
        }

        /// <summary>
        /// 读取本机构建历史文件
        /// </summary>
        private static HistoryDocument Load(string path)
        {
            if (!File.Exists(path)) return new HistoryDocument();
            HistoryDocument history = JsonUtility.FromJson<HistoryDocument>(File.ReadAllText(path));
            if (history == null) return new HistoryDocument();
            history.records ??= new List<AutoBuildBuildRecord>();
            return history;
        }
    }

    /// <summary>
    /// 输出自动构建文本日志
    /// </summary>
    internal static class AutoBuildRunLogger
    {
        /// <summary>
        /// 追加一条带本机时间的日志
        /// </summary>
        public static void Write(string path, string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.AppendAllText(path, line + Environment.NewLine);
            Debug.Log($"[AutoBuild] {message}");
        }
    }
}
#endif
