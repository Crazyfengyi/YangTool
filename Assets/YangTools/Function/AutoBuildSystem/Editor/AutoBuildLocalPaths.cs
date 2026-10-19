#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 获取自动构建用户本机数据目录
    /// </summary>
    internal static class AutoBuildLocalPaths
    {
        /// <summary>
        /// 返回当前 Unity 项目的本机配置目录
        /// </summary>
        public static string GetProjectDirectory()
        {
            string projectPath = Path.GetFullPath(Application.dataPath);
            string projectHash;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(projectPath.ToLowerInvariant()));
                projectHash = BitConverter.ToString(digest, 0, 8).Replace("-", string.Empty).ToLowerInvariant();
            }

            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YangTools", "AutoBuildSystem", projectHash);
            Directory.CreateDirectory(root);
            return root;
        }

        /// <summary>
        /// 获取 DPAPI 加密的凭据文件路径
        /// </summary>
        public static string GetSecretFilePath()
        {
            return Path.Combine(GetProjectDirectory(), "credentials.dat");
        }

        /// <summary>
        /// 获取当前构建事务快照路径
        /// </summary>
        public static string GetSnapshotFilePath()
        {
            return Path.Combine(GetProjectDirectory(), "settings-snapshot.dat");
        }

        /// <summary>
        /// 获取本机定时器状态文件路径
        /// </summary>
        public static string GetSchedulerFilePath()
        {
            return Path.Combine(GetProjectDirectory(), "scheduler-state.json");
        }

        /// <summary>
        /// 获取跨脚本重载保存的构建事务路径
        /// </summary>
        public static string GetJobStateFilePath()
        {
            return Path.Combine(GetProjectDirectory(), "active-job.json");
        }

        /// <summary>
        /// 获取本机构建历史文件路径
        /// </summary>
        public static string GetHistoryFilePath()
        {
            return Path.Combine(GetProjectDirectory(), "history.json");
        }
    }
}
#endif
