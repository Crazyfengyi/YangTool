#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 构建事务中需要临时修改的 PlayerSettings 快照
    /// </summary>
    [Serializable]
    internal sealed class AutoBuildSettingsSnapshot
    {
        public string companyName;
        public string productName;
        public string bundleVersion;
        public int activeBuildTarget;
        public bool buildAppBundle;
        public string keystoreName;
        public string keystorePass;
        public string keyaliasName;
        public string keyaliasPass;
        public bool useCustomKeystore;
        public int androidVersionCode;
        public string iosBuildNumber;
        public List<AutoBuildPlatformSnapshot> platforms = new List<AutoBuildPlatformSnapshot>();

        /// <summary>
        /// 捕获当前项目设置并安全写入本机 DPAPI 文件
        /// </summary>
        public static void CaptureAndSave()
        {
            AutoBuildSettingsSnapshot snapshot = new AutoBuildSettingsSnapshot
            {
                companyName = PlayerSettings.companyName,
                productName = PlayerSettings.productName,
                bundleVersion = PlayerSettings.bundleVersion,
                activeBuildTarget = (int)EditorUserBuildSettings.activeBuildTarget,
                buildAppBundle = EditorUserBuildSettings.buildAppBundle,
                keystoreName = PlayerSettings.Android.keystoreName,
                keystorePass = PlayerSettings.Android.keystorePass,
                keyaliasName = PlayerSettings.Android.keyaliasName,
                keyaliasPass = PlayerSettings.Android.keyaliasPass,
                useCustomKeystore = PlayerSettings.Android.useCustomKeystore,
                androidVersionCode = PlayerSettings.Android.bundleVersionCode,
                iosBuildNumber = PlayerSettings.iOS.buildNumber
            };

            foreach (AutoBuildPlatform platform in Enum.GetValues(typeof(AutoBuildPlatform)))
            {
                snapshot.platforms.Add(AutoBuildPlatformSnapshot.Capture(platform));
            }

            AutoBuildSecretStore.WriteEncrypted(AutoBuildLocalPaths.GetSnapshotFilePath(),
                System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(snapshot)));
        }

        /// <summary>
        /// 从加密文件恢复构建前项目设置
        /// </summary>
        public static bool RestoreAndDelete()
        {
            string path = AutoBuildLocalPaths.GetSnapshotFilePath();
            if (!File.Exists(path)) return false;

            string json = System.Text.Encoding.UTF8.GetString(AutoBuildSecretStore.ReadEncrypted(path));
            AutoBuildSettingsSnapshot snapshot = JsonUtility.FromJson<AutoBuildSettingsSnapshot>(json);
            if (snapshot == null) throw new InvalidDataException("构建设置快照内容无效");

            PlayerSettings.companyName = snapshot.companyName;
            PlayerSettings.productName = snapshot.productName;
            PlayerSettings.bundleVersion = snapshot.bundleVersion;
            PlayerSettings.Android.keystoreName = snapshot.keystoreName;
            PlayerSettings.Android.keystorePass = snapshot.keystorePass;
            PlayerSettings.Android.keyaliasName = snapshot.keyaliasName;
            PlayerSettings.Android.keyaliasPass = snapshot.keyaliasPass;
            PlayerSettings.Android.useCustomKeystore = snapshot.useCustomKeystore;
            PlayerSettings.Android.bundleVersionCode = snapshot.androidVersionCode;
            PlayerSettings.iOS.buildNumber = snapshot.iosBuildNumber;
            EditorUserBuildSettings.buildAppBundle = snapshot.buildAppBundle;

            foreach (AutoBuildPlatformSnapshot platform in snapshot.platforms)
            {
                platform.Restore();
            }

            AutoBuildSecretStore.DeleteEncryptedFile(path);
            AssetDatabase.SaveAssets();
            BuildTarget originalTarget = (BuildTarget)snapshot.activeBuildTarget;
            if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildPipeline.GetBuildTargetGroup(originalTarget), originalTarget);
            }

            return true;
        }
    }

    /// <summary>
    /// 单个平台的应用标识和图标快照
    /// </summary>
    [Serializable]
    internal sealed class AutoBuildPlatformSnapshot
    {
        public AutoBuildPlatform platform;
        public string applicationIdentifier;
        public List<string> iconAssetPaths = new List<string>();

        /// <summary>
        /// 捕获指定平台的应用标识和图标
        /// </summary>
        public static AutoBuildPlatformSnapshot Capture(AutoBuildPlatform platform)
        {
            NamedBuildTarget namedTarget = AutoBuildPlayerBuilder.ToNamedBuildTarget(platform);
            AutoBuildPlatformSnapshot snapshot = new AutoBuildPlatformSnapshot
            {
                platform = platform,
                applicationIdentifier = PlayerSettings.GetApplicationIdentifier(namedTarget)
            };

            try
            {
                Texture2D[] icons = PlayerSettings.GetIcons(namedTarget, IconKind.Application);
                if (icons != null)
                {
                    foreach (Texture2D icon in icons) snapshot.iconAssetPaths.Add(AssetDatabase.GetAssetPath(icon));
                }
            }
            catch (Exception)
            {
                // 平台模块缺失时仍保留其他设置快照
            }

            return snapshot;
        }

        /// <summary>
        /// 恢复指定平台的应用标识和图标
        /// </summary>
        public void Restore()
        {
            NamedBuildTarget namedTarget = AutoBuildPlayerBuilder.ToNamedBuildTarget(platform);
            PlayerSettings.SetApplicationIdentifier(namedTarget, applicationIdentifier ?? string.Empty);
            try
            {
                Texture2D[] icons = new Texture2D[iconAssetPaths.Count];
                for (int i = 0; i < iconAssetPaths.Count; i++)
                {
                    icons[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(iconAssetPaths[i]);
                }

                PlayerSettings.SetIcons(namedTarget, icons, IconKind.Application);
            }
            catch (Exception)
            {
                // 平台模块缺失时不阻止其余 PlayerSettings 恢复
            }
        }
    }
}
#endif
