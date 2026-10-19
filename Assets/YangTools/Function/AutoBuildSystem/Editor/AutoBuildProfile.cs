#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 自动构建使用的平台类型
    /// </summary>
    public enum AutoBuildPlatform
    {
        Android,
        IOS,
        Windows64,
        WebGL
    }

    /// <summary>
    /// Android 应用产物格式
    /// </summary>
    public enum AutoBuildAndroidFormat
    {
        Apk,
        AppBundle
    }

    /// <summary>
    /// 自动构建计划周期
    /// </summary>
    public enum AutoBuildScheduleMode
    {
        Daily,
        Weekly
    }

    /// <summary>
    /// 自动构建平台配置
    /// </summary>
    [Serializable]
    public sealed class AutoBuildPlatformSettings
    {
        public AutoBuildPlatform platform;
        public bool enabled;
        public string applicationIdentifier;
        public Texture2D icon;
        public AutoBuildAndroidFormat androidFormat;
        public bool useCustomKeystore;
        public string keystorePath;
        public string keyAlias;
    }

    /// <summary>
    /// YooAsset 自动构建参数
    /// </summary>
    [Serializable]
    public sealed class AutoBuildBundleSettings
    {
        public bool enabled;
        public string packageName = "DefaultPackage";
        public string packageVersion = "1.0.0";
        public EBuildPipeline pipeline = EBuildPipeline.ScriptableBuildPipeline;
        public ECompressOption compression = ECompressOption.LZ4;
        public EFileNameStyle fileNameStyle = EFileNameStyle.HashName;
        public EBuildinFileCopyOption buildinFileCopyOption = EBuildinFileCopyOption.None;
        public string buildinFileCopyParams;
        public bool clearBuildCacheFiles;
        public bool verifyBuildingResult = true;
        public bool useAssetDependencyDB = true;
    }

    /// <summary>
    /// Editor 内定时构建配置
    /// </summary>
    [Serializable]
    public sealed class AutoBuildScheduleSettings
    {
        public bool enabled;
        public AutoBuildScheduleMode mode;
        public int hour = 2;
        public int minute;
        public DayOfWeek dayOfWeek = DayOfWeek.Monday;
    }

    /// <summary>
    /// SMTP 邮件通知配置
    /// </summary>
    [Serializable]
    public sealed class AutoBuildEmailSettings
    {
        public bool enabled;
        public string host;
        public int port = 587;
        public string sender;
        public string recipient;
        public string username;
        public bool enableTls = true;
    }

    /// <summary>
    /// 可复用的自动构建档案
    /// </summary>
    [CreateAssetMenu(menuName = "YangTools/自动构建/构建档案", fileName = "AutoBuildProfile")]
    public sealed class AutoBuildProfile : ScriptableObject
    {
        public string profileId;
        public string profileName = "新建构建档案";
        public string companyName = "YangTools";
        public string productName = "YangTools";
        public string version = "1.0.0";
        public int buildNumberCode = 1;
        public string outputRoot = "Builds/AutoBuild";
        public List<AutoBuildPlatformSettings> platforms = new List<AutoBuildPlatformSettings>();
        public AutoBuildBundleSettings bundles = new AutoBuildBundleSettings();
        public AutoBuildScheduleSettings schedule = new AutoBuildScheduleSettings();
        public AutoBuildEmailSettings email = new AutoBuildEmailSettings();

        /// <summary>
        /// 确保档案拥有稳定的本机凭据标识和平台条目
        /// </summary>
        public void EnsureDefaults()
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                profileId = Guid.NewGuid().ToString("N");
            }

            if (platforms == null)
            {
                platforms = new List<AutoBuildPlatformSettings>();
            }

            foreach (AutoBuildPlatform platform in Enum.GetValues(typeof(AutoBuildPlatform)))
            {
                if (GetPlatform(platform) == null)
                {
                    platforms.Add(new AutoBuildPlatformSettings
                    {
                        platform = platform,
                        enabled = platform == AutoBuildPlatform.Android,
                        androidFormat = AutoBuildAndroidFormat.Apk
                    });
                }
            }

            bundles ??= new AutoBuildBundleSettings();
            schedule ??= new AutoBuildScheduleSettings();
            email ??= new AutoBuildEmailSettings();
        }

        /// <summary>
        /// 查找指定平台配置
        /// </summary>
        public AutoBuildPlatformSettings GetPlatform(AutoBuildPlatform platform)
        {
            return platforms?.Find(item => item != null && item.platform == platform);
        }

        /// <summary>
        /// 获取指定平台对应的 Unity 构建目标
        /// </summary>
        public static BuildTarget ToBuildTarget(AutoBuildPlatform platform)
        {
            switch (platform)
            {
                case AutoBuildPlatform.Android: return BuildTarget.Android;
                case AutoBuildPlatform.IOS: return BuildTarget.iOS;
                case AutoBuildPlatform.Windows64: return BuildTarget.StandaloneWindows64;
                case AutoBuildPlatform.WebGL: return BuildTarget.WebGL;
                default: throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }

        /// <summary>
        /// 获取平台显示名称
        /// </summary>
        public static string GetPlatformDisplayName(AutoBuildPlatform platform)
        {
            switch (platform)
            {
                case AutoBuildPlatform.Android: return "Android";
                case AutoBuildPlatform.IOS: return "iOS (Xcode 工程)";
                case AutoBuildPlatform.Windows64: return "Windows x64";
                case AutoBuildPlatform.WebGL: return "WebGL";
                default: return platform.ToString();
            }
        }
    }
}
#endif
