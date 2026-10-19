#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 执行可跨 Unity 脚本重载恢复的本机构建事务
    /// </summary>
    [InitializeOnLoad]
    internal static class AutoBuildRunner
    {
        private const string SessionKey = "YangTools.AutoBuildSystem.ActiveJob";
        private static AutoBuildJobState activeJob;

        #region 初始化与入口

        static AutoBuildRunner()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            LoadOrRecoverJob();
        }

        /// <summary>
        /// 获取当前构建状态
        /// </summary>
        public static bool IsRunning => activeJob != null;

        /// <summary>
        /// 获取当前构建步骤文字
        /// </summary>
        public static string CurrentStatus => activeJob == null ? "空闲" : $"{activeJob.profileName} / {activeJob.stage}";

        /// <summary>
        /// 启动指定构建档案
        /// </summary>
        public static bool TryStart(AutoBuildProfile profile, bool scheduled, out string error)
        {
            error = string.Empty;
            if (IsRunning)
            {
                error = "已有自动构建正在运行";
                return false;
            }

            profile?.EnsureDefaults();
            List<string> validationErrors = AutoBuildProfileValidator.Validate(profile);
            if (validationErrors.Count > 0)
            {
                error = string.Join("\n", validationErrors);
                RecordPreflightFailure(profile, error);
                return false;
            }

            List<AutoBuildPlatform> targets = profile.platforms
                .Where(item => item != null && item.enabled)
                .Select(item => item.platform)
                .Distinct()
                .ToList();
            string profilePath = AssetDatabase.GetAssetPath(profile);
            string runId = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            string root = ResolveOutputRoot(profile.outputRoot);
            string runRoot = Path.Combine(root, MakeSafeName(profile.profileName), runId);
            string logPath = Path.Combine(runRoot, "Logs", "build.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            activeJob = new AutoBuildJobState
            {
                profilePath = profilePath,
                profileName = profile.profileName,
                runId = runId,
                runRoot = runRoot,
                logPath = logPath,
                startedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                startedAtUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                targetValues = targets.Select(item => (int)item).ToArray(),
                originalBuildTarget = (int)EditorUserBuildSettings.activeBuildTarget,
                stage = AutoBuildJobStage.SwitchToTarget,
                scheduled = scheduled
            };

            try
            {
                AutoBuildSettingsSnapshot.CaptureAndSave();
                WriteLog("构建事务启动");
                PersistJob();
                return true;
            }
            catch (Exception exception)
            {
                error = "无法保存构建前设置快照：" + exception.Message;
                activeJob = null;
                SessionState.EraseString(SessionKey);
                RecordPreflightFailure(profile, error);
                return false;
            }
        }

        #endregion

        #region 平台切换与构建

        /// <summary>
        /// 每帧推进构建事务状态
        /// </summary>
        private static void Update()
        {
            if (activeJob == null) return;
            try
            {
                switch (activeJob.stage)
                {
                    case AutoBuildJobStage.SwitchToTarget:
                        ProcessTargetSwitch();
                        break;
                    case AutoBuildJobStage.BuildTarget:
                        ProcessBuildTarget();
                        break;
                    case AutoBuildJobStage.Building:
                        FailAndRestore("构建期间发生脚本重载或中断，本次构建无法安全续跑");
                        break;
                    case AutoBuildJobStage.RestoreSettings:
                        ProcessRestore();
                        break;
                }
            }
            catch (Exception exception)
            {
                FailAndRestore(exception.ToString());
            }
        }

        /// <summary>
        /// 在目标平台切换完成后进入构建步骤
        /// </summary>
        private static void ProcessTargetSwitch()
        {
            if (activeJob.recovering || activeJob.targetIndex >= activeJob.targetValues.Length)
            {
                activeJob.stage = AutoBuildJobStage.RestoreSettings;
                PersistJob();
                return;
            }

            BuildTarget target = AutoBuildProfile.ToBuildTarget((AutoBuildPlatform)activeJob.targetValues[activeJob.targetIndex]);
            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
                activeJob.switchRequested = false;
                activeJob.stage = AutoBuildJobStage.BuildTarget;
                PersistJob();
                return;
            }

            if (activeJob.switchRequested)
            {
                DateTime requestedAt = DateTime.Parse(activeJob.switchRequestedAt, null, System.Globalization.DateTimeStyles.RoundtripKind);
                if (DateTime.UtcNow - requestedAt > TimeSpan.FromMinutes(5))
                {
                    throw new TimeoutException($"切换构建平台超时：{target}");
                }

                return;
            }

            bool accepted = EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildPipeline.GetBuildTargetGroup(target), target);
            if (!accepted) throw new InvalidOperationException($"Unity 未能切换到构建平台：{target}");
            activeJob.switchRequested = true;
            activeJob.switchRequestedAt = DateTime.UtcNow.ToString("o");
            PersistJob();
            WriteLog($"请求切换构建平台：{target}");
        }

        /// <summary>
        /// 构建当前平台的 AB 包和 Player
        /// </summary>
        private static void ProcessBuildTarget()
        {
            AutoBuildProfile profile = AssetDatabase.LoadAssetAtPath<AutoBuildProfile>(activeJob.profilePath);
            if (profile == null) throw new FileNotFoundException("构建档案资源丢失", activeJob.profilePath);

            AutoBuildPlatform platform = (AutoBuildPlatform)activeJob.targetValues[activeJob.targetIndex];
            activeJob.stage = AutoBuildJobStage.Building;
            PersistJob();
            WriteLog($"平台开始：{AutoBuildProfile.GetPlatformDisplayName(platform)}");
            List<string> outputs = AutoBuildPlayerBuilder.Build(profile, platform, activeJob.runRoot, activeJob.runId, WriteLog);
            activeJob.outputPaths.AddRange(outputs);
            activeJob.targetIndex++;
            activeJob.switchRequested = false;
            activeJob.stage = activeJob.targetIndex >= activeJob.targetValues.Length
                ? AutoBuildJobStage.RestoreSettings
                : AutoBuildJobStage.SwitchToTarget;
            PersistJob();
        }

        #endregion

        #region 设置恢复与构建结果

        /// <summary>
        /// 恢复项目原构建目标和 PlayerSettings
        /// </summary>
        private static void ProcessRestore()
        {
            BuildTarget originalTarget = (BuildTarget)activeJob.originalBuildTarget;
            if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
            {
                if (activeJob.switchRequested)
                {
                    DateTime requestedAt = DateTime.Parse(activeJob.switchRequestedAt, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    if (DateTime.UtcNow - requestedAt > TimeSpan.FromMinutes(5))
                        throw new TimeoutException($"恢复原构建平台超时：{originalTarget}");
                    return;
                }

                bool accepted = EditorUserBuildSettings.SwitchActiveBuildTargetAsync(
                    BuildPipeline.GetBuildTargetGroup(originalTarget), originalTarget);
                if (!accepted) throw new InvalidOperationException($"Unity 未能恢复原构建平台：{originalTarget}");
                activeJob.switchRequested = true;
                activeJob.switchRequestedAt = DateTime.UtcNow.ToString("o");
                PersistJob();
                return;
            }

            bool wasRecovering = activeJob.recovering;
            string restoreError = string.Empty;
            try
            {
                AutoBuildSettingsSnapshot.RestoreAndDelete();
            }
            catch (Exception exception)
            {
                restoreError = "恢复项目设置失败：" + exception.Message;
            }

            if (!string.IsNullOrEmpty(restoreError))
            {
                activeJob.error = string.IsNullOrEmpty(activeJob.error) ? restoreError : activeJob.error + "\n" + restoreError;
            }
            else
            {
                WriteLog("项目 PlayerSettings 与活动构建平台已恢复");
            }

            Finish(wasRecovering ? false : string.IsNullOrEmpty(activeJob.error),
                wasRecovering && string.IsNullOrEmpty(activeJob.error) ? "Unity 编辑器在构建过程中关闭，本次构建已中断" : activeJob.error);
        }

        /// <summary>
        /// 将构建错误转入恢复步骤
        /// </summary>
        private static void FailAndRestore(string message)
        {
            if (activeJob == null) return;
            activeJob.error = message;
            activeJob.stage = AutoBuildJobStage.RestoreSettings;
            activeJob.switchRequested = false;
            WriteLog("构建失败：" + message);
            PersistJob();
        }

        /// <summary>
        /// 写入最终记录并发送邮件通知
        /// </summary>
        private static void Finish(bool succeeded, string message)
        {
            AutoBuildJobState finishedJob = activeJob;
            AutoBuildProfile profile = AssetDatabase.LoadAssetAtPath<AutoBuildProfile>(finishedJob.profilePath);
            string status = succeeded ? AutoBuildResultStatus.Succeeded.ToString() : AutoBuildResultStatus.Failed.ToString();
            if (!string.IsNullOrEmpty(message)) WriteLog("构建结束：" + message);

            AutoBuildBuildRecord record = new AutoBuildBuildRecord
            {
                runId = finishedJob.runId,
                profileName = finishedJob.profileName,
                startedAt = finishedJob.startedAt,
                finishedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                durationSeconds = Math.Max(0, (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - finishedJob.startedAtUnixMilliseconds) / 1000.0),
                status = status,
                message = string.IsNullOrWhiteSpace(message) ? "构建完成" : message,
                logPath = finishedJob.logPath,
                outputPaths = finishedJob.outputPaths
            };
            AutoBuildHistoryStore.Add(record);
            activeJob = null;
            SessionState.EraseString(SessionKey);
            AutoBuildSecretStore.DeleteEncryptedFile(AutoBuildLocalPaths.GetJobStateFilePath());
            if (profile != null && profile.email != null && profile.email.enabled)
            {
                SendNotification(profile, record);
            }
        }

        #endregion

        #region 失败记录与通知

        /// <summary>
        /// 处理档案校验失败并记录错误
        /// </summary>
        private static void RecordPreflightFailure(AutoBuildProfile profile, string error)
        {
            string runId = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_validation";
            string logPath = Path.Combine(AutoBuildLocalPaths.GetProjectDirectory(), "logs", runId + ".log");
            AutoBuildRunLogger.Write(logPath, "构建未启动：" + error);
            AutoBuildBuildRecord record = new AutoBuildBuildRecord
            {
                runId = runId,
                profileName = profile == null ? "(无档案)" : profile.profileName,
                startedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                finishedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                status = AutoBuildResultStatus.Failed.ToString(),
                message = error,
                logPath = logPath
            };
            AutoBuildHistoryStore.Add(record);
            if (profile?.email != null && profile.email.enabled) SendNotification(profile, record);
        }

        /// <summary>
        /// 在后台发送通知并将异常写入构建日志
        /// </summary>
        private static async void SendNotification(AutoBuildProfile profile, AutoBuildBuildRecord record)
        {
            try
            {
                await AutoBuildNotifier.SendCompletionAsync(profile, record);
                AutoBuildRunLogger.Write(record.logPath, "邮件通知已发送");
            }
            catch (Exception exception)
            {
                AutoBuildRunLogger.Write(record.logPath, "邮件通知失败：" + exception.Message);
            }
        }

        #endregion

        #region 事务恢复与辅助方法

        /// <summary>
        /// 从 Unity 会话或本机事务文件恢复构建状态
        /// </summary>
        private static void LoadOrRecoverJob()
        {
            string json = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                activeJob = JsonUtility.FromJson<AutoBuildJobState>(json);
                return;
            }

            string jobPath = AutoBuildLocalPaths.GetJobStateFilePath();
            if (!File.Exists(jobPath)) return;
            activeJob = JsonUtility.FromJson<AutoBuildJobState>(File.ReadAllText(jobPath));
            if (activeJob == null)
            {
                AutoBuildSecretStore.DeleteEncryptedFile(jobPath);
                return;
            }

            activeJob.recovering = true;
            activeJob.error = "Unity 编辑器在构建过程中关闭，本次构建已中断";
            activeJob.stage = AutoBuildJobStage.RestoreSettings;
            activeJob.switchRequested = false;
            PersistJob();
        }

        /// <summary>
        /// 保存不含敏感信息的事务状态
        /// </summary>
        private static void PersistJob()
        {
            if (activeJob == null) return;
            string json = JsonUtility.ToJson(activeJob);
            SessionState.SetString(SessionKey, json);
            File.WriteAllText(AutoBuildLocalPaths.GetJobStateFilePath(), json);
        }

        /// <summary>
        /// 输出当前事务日志
        /// </summary>
        private static void WriteLog(string message)
        {
            if (activeJob != null) AutoBuildRunLogger.Write(activeJob.logPath, message);
        }

        /// <summary>
        /// 将输出目录解析为本机绝对路径
        /// </summary>
        private static string ResolveOutputRoot(string outputRoot)
        {
            if (Path.IsPathRooted(outputRoot)) return Path.GetFullPath(outputRoot);
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, outputRoot));
        }

        /// <summary>
        /// 清理不适用于 Windows 文件夹名的字符
        /// </summary>
        private static string MakeSafeName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return string.IsNullOrWhiteSpace(value) ? "BuildProfile" : value;
        }

        #endregion
    }

    /// <summary>
    /// 跨脚本重载保存的自动构建进度
    /// </summary>
    [Serializable]
    internal sealed class AutoBuildJobState
    {
        public string profilePath;
        public string profileName;
        public string runId;
        public string runRoot;
        public string logPath;
        public string startedAt;
        public long startedAtUnixMilliseconds;
        public string error;
        public string switchRequestedAt;
        public int[] targetValues = Array.Empty<int>();
        public int targetIndex;
        public int originalBuildTarget;
        public AutoBuildJobStage stage;
        public bool switchRequested;
        public bool scheduled;
        public bool recovering;
        public List<string> outputPaths = new List<string>();
    }

    /// <summary>
    /// 构建事务内部步骤
    /// </summary>
    internal enum AutoBuildJobStage
    {
        SwitchToTarget,
        BuildTarget,
        Building,
        RestoreSettings
    }
}
#endif
