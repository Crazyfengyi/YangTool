#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 仅在 Unity Editor 打开期间检查本机定时计划
    /// </summary>
    [InitializeOnLoad]
    internal static class AutoBuildScheduler
    {
        private const string ProfileFolder = "Assets/YangTools/Function/AutoBuildSystem/Profiles";
        private static readonly List<AutoBuildScheduleEntry> entries = new List<AutoBuildScheduleEntry>();
        private static DateTime nextCheckAt;
        private static DateTime lastTickAt;

        static AutoBuildScheduler()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.projectChanged -= Refresh;
            EditorApplication.projectChanged += Refresh;
            Refresh();
        }

        /// <summary>
        /// 重新加载档案并跳过 Unity 未运行期间错过的计划
        /// </summary>
        public static void Refresh()
        {
            lastTickAt = DateTime.Now;
            AutoBuildScheduleState state = LoadState();
            Dictionary<string, AutoBuildScheduleEntry> previous = new Dictionary<string, AutoBuildScheduleEntry>();
            foreach (AutoBuildScheduleEntry entry in state.entries)
            {
                previous[entry.profilePath] = entry;
            }

            entries.Clear();
            string[] assetIds = AssetDatabase.FindAssets("t:AutoBuildProfile", new[] { ProfileFolder });
            DateTime now = DateTime.Now;
            foreach (string assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                AutoBuildProfile profile = AssetDatabase.LoadAssetAtPath<AutoBuildProfile>(path);
                if (profile == null || profile.schedule == null || !profile.schedule.enabled) continue;

                string signature = GetScheduleSignature(profile);
                AutoBuildScheduleEntry entry;
                if (!previous.TryGetValue(path, out entry) || entry.signature != signature ||
                    !DateTime.TryParse(entry.nextRunAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out _))
                {
                    entry = new AutoBuildScheduleEntry
                    {
                        profilePath = path,
                        profileName = profile.profileName,
                        signature = signature,
                        nextRunAt = AutoBuildScheduleCalculator.GetNextRun(now, profile.schedule).ToString("o")
                    };
                }
                else
                {
                    entry.profileName = profile.profileName;
                    DateTime nextRun = DateTime.Parse(entry.nextRunAt, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    if (nextRun <= now)
                    {
                        AddMissedRecord(profile, nextRun);
                        entry.nextRunAt = AutoBuildScheduleCalculator.GetNextRun(now, profile.schedule).ToString("o");
                    }
                }

                entries.Add(entry);
            }

            SaveState();
        }

        /// <summary>
        /// 定期检查档案是否到达构建时间
        /// </summary>
        private static void Tick()
        {
            DateTime now = DateTime.Now;
            if (now < nextCheckAt) return;
            bool editorWasPaused = lastTickAt != default &&
                now - lastTickAt > TimeSpan.FromSeconds(AutoBuildScheduleCalculator.MaxScheduleLatenessSeconds);
            lastTickAt = now;
            nextCheckAt = now.AddSeconds(1);

            foreach (AutoBuildScheduleEntry entry in entries)
            {
                if (!DateTime.TryParse(entry.nextRunAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime scheduledAt) ||
                    now < scheduledAt)
                {
                    continue;
                }

                AutoBuildProfile profile = AssetDatabase.LoadAssetAtPath<AutoBuildProfile>(entry.profilePath);
                if (profile == null || profile.schedule == null || !profile.schedule.enabled) continue;

                entry.nextRunAt = AutoBuildScheduleCalculator.GetNextRun(now, profile.schedule).ToString("o");
                SaveState();
                if (editorWasPaused || AutoBuildScheduleCalculator.IsMissed(scheduledAt, now))
                {
                    AddMissedRecord(profile, scheduledAt);
                }
                else if (AutoBuildRunner.IsRunning)
                {
                    AddSkippedRecord(profile, scheduledAt, "已有构建正在运行");
                }
                else
                {
                    AutoBuildRunner.TryStart(profile, true, out _);
                }

                break;
            }
        }

        /// <summary>
        /// 记录 Unity 未运行期间错过的计划
        /// </summary>
        private static void AddMissedRecord(AutoBuildProfile profile, DateTime scheduledAt)
        {
            AddSkippedRecord(profile, scheduledAt, "Unity Editor 未运行或电脑休眠，计划未补跑");
        }

        /// <summary>
        /// 追加一条跳过计划的本机历史
        /// </summary>
        private static void AddSkippedRecord(AutoBuildProfile profile, DateTime scheduledAt, string reason)
        {
            AutoBuildHistoryStore.Add(new AutoBuildBuildRecord
            {
                runId = "skip_" + scheduledAt.ToString("yyyyMMdd_HHmmss"),
                profileName = profile.profileName,
                startedAt = scheduledAt.ToString("yyyy-MM-dd HH:mm:ss"),
                finishedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                status = AutoBuildResultStatus.Skipped.ToString(),
                message = reason
            });
        }

        /// <summary>
        /// 构造计划配置签名用于识别计划修改
        /// </summary>
        private static string GetScheduleSignature(AutoBuildProfile profile)
        {
            return $"{profile.profileId}|{profile.schedule.mode}|{profile.schedule.hour}|{profile.schedule.minute}|{profile.schedule.dayOfWeek}";
        }

        /// <summary>
        /// 读取本机保存的计划时间
        /// </summary>
        private static AutoBuildScheduleState LoadState()
        {
            string path = AutoBuildLocalPaths.GetSchedulerFilePath();
            if (!File.Exists(path)) return new AutoBuildScheduleState();
            AutoBuildScheduleState state = JsonUtility.FromJson<AutoBuildScheduleState>(File.ReadAllText(path));
            if (state == null) return new AutoBuildScheduleState();
            state.entries ??= new List<AutoBuildScheduleEntry>();
            return state;
        }

        /// <summary>
        /// 保存下一次计划时间到本机目录
        /// </summary>
        private static void SaveState()
        {
            string path = AutoBuildLocalPaths.GetSchedulerFilePath();
            File.WriteAllText(path, JsonUtility.ToJson(new AutoBuildScheduleState { entries = entries }));
        }
    }

    /// <summary>
    /// 本机单个档案的下一次计划记录
    /// </summary>
    [Serializable]
    internal sealed class AutoBuildScheduleEntry
    {
        public string profilePath;
        public string profileName;
        public string signature;
        public string nextRunAt;
    }

    /// <summary>
    /// 本机定时构建状态文档
    /// </summary>
    [Serializable]
    internal sealed class AutoBuildScheduleState
    {
        public List<AutoBuildScheduleEntry> entries = new List<AutoBuildScheduleEntry>();
    }
}
#endif
