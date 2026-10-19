#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 应用档案设置并构建单个平台 Player
    /// </summary>
    internal static class AutoBuildPlayerBuilder
    {
        /// <summary>
        /// 为一个平台生成 YooAsset 资源和 Player 产物
        /// </summary>
        public static System.Collections.Generic.List<string> Build(AutoBuildProfile profile, AutoBuildPlatform platform, string runRoot, string runId,
            Action<string> log)
        {
            System.Collections.Generic.List<string> outputPaths = new System.Collections.Generic.List<string>();
            AutoBuildPlatformSettings settings = profile.GetPlatform(platform);
            ApplySettings(profile, settings);
            BuildTarget target = AutoBuildProfile.ToBuildTarget(platform);
            string platformRoot = Path.Combine(runRoot, platform.ToString());
            Directory.CreateDirectory(platformRoot);

            if (profile.bundles.enabled)
            {
                log("开始构建 YooAsset 资源包");
                string bundlePath = YooAssetBuildRunner.Build(profile, platform,
                    Path.Combine(runRoot, "AssetBundles"), runId);
                outputPaths.Add(bundlePath);
                log($"YooAsset 输出：{bundlePath}");
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("Build Settings 中没有启用的场景");

            string outputPath = GetPlayerOutputPath(profile, settings, platformRoot);
            BuildOptions options = BuildOptions.StrictMode;
            if (platform == AutoBuildPlatform.Android && settings.androidFormat == AutoBuildAndroidFormat.AppBundle)
            {
                EditorUserBuildSettings.buildAppBundle = true;
            }
            else
            {
                EditorUserBuildSettings.buildAppBundle = false;
            }

            log($"开始构建 {AutoBuildProfile.GetPlatformDisplayName(platform)}");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                locationPathName = outputPath,
                options = options
            });

            if (report == null || report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                string result = report == null ? "未返回构建报告" : $"{report.summary.result}: {report.summary.totalErrors} 个错误";
                throw new InvalidOperationException($"Player 构建失败 {result}");
            }

            string artifactPath = ValidateOutput(platform, outputPath);
            outputPaths.Add(artifactPath);
            log($"构建成功 耗时 {report.summary.totalTime} 产物：{artifactPath}");
            return outputPaths;
        }

        /// <summary>
        /// 映射自动构建平台到 Unity 的命名构建目标
        /// </summary>
        public static NamedBuildTarget ToNamedBuildTarget(AutoBuildPlatform platform)
        {
            switch (platform)
            {
                case AutoBuildPlatform.Android: return NamedBuildTarget.Android;
                case AutoBuildPlatform.IOS: return NamedBuildTarget.iOS;
                case AutoBuildPlatform.Windows64: return NamedBuildTarget.Standalone;
                case AutoBuildPlatform.WebGL: return NamedBuildTarget.WebGL;
                default: throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }

        private static void ApplySettings(AutoBuildProfile profile, AutoBuildPlatformSettings settings)
        {
            PlayerSettings.companyName = profile.companyName;
            PlayerSettings.productName = profile.productName;
            PlayerSettings.bundleVersion = profile.version;
            NamedBuildTarget namedTarget = ToNamedBuildTarget(settings.platform);
            if (!string.IsNullOrWhiteSpace(settings.applicationIdentifier))
            {
                PlayerSettings.SetApplicationIdentifier(namedTarget, settings.applicationIdentifier);
            }

            if (settings.icon != null)
            {
                int[] iconSizes = PlayerSettings.GetIconSizes(namedTarget, IconKind.Application);
                if (iconSizes.Length > 0)
                {
                    PlayerSettings.SetIcons(namedTarget, iconSizes.Select(_ => settings.icon).ToArray(), IconKind.Application);
                }
            }

            if (settings.platform == AutoBuildPlatform.Android)
            {
                PlayerSettings.Android.bundleVersionCode = profile.buildNumberCode;
                PlayerSettings.Android.useCustomKeystore = settings.useCustomKeystore;
                if (settings.useCustomKeystore)
                {
                    string keystorePassword = AutoBuildSecretStore.Get(AutoBuildCredentialKeys.ForProfile(profile, "android.keystore"));
                    string aliasPassword = AutoBuildSecretStore.Get(AutoBuildCredentialKeys.ForProfile(profile, "android.alias"));
                    if (string.IsNullOrEmpty(keystorePassword) || string.IsNullOrEmpty(aliasPassword))
                    {
                        throw new InvalidOperationException("Android Keystore 密码尚未保存在本机凭据中");
                    }

                    PlayerSettings.Android.keystoreName = settings.keystorePath;
                    PlayerSettings.Android.keystorePass = keystorePassword;
                    PlayerSettings.Android.keyaliasName = settings.keyAlias;
                    PlayerSettings.Android.keyaliasPass = aliasPassword;
                }
            }
            else if (settings.platform == AutoBuildPlatform.IOS)
            {
                PlayerSettings.iOS.buildNumber = profile.buildNumberCode.ToString();
            }
        }

        private static string GetPlayerOutputPath(AutoBuildProfile profile, AutoBuildPlatformSettings settings, string platformRoot)
        {
            string safeName = MakeSafeFileName(profile.productName);
            switch (settings.platform)
            {
                case AutoBuildPlatform.Android:
                    string extension = settings.androidFormat == AutoBuildAndroidFormat.AppBundle ? ".aab" : ".apk";
                    return Path.Combine(platformRoot, safeName + extension);
                case AutoBuildPlatform.IOS:
                    return Path.Combine(platformRoot, "XcodeProject");
                case AutoBuildPlatform.Windows64:
                    return Path.Combine(platformRoot, safeName + ".exe");
                case AutoBuildPlatform.WebGL:
                    return Path.Combine(platformRoot, "WebGL");
                default:
                    throw new ArgumentOutOfRangeException(nameof(settings.platform));
            }
        }

        private static string ValidateOutput(AutoBuildPlatform platform, string outputPath)
        {
            bool exists;
            switch (platform)
            {
                case AutoBuildPlatform.Android:
                case AutoBuildPlatform.Windows64:
                    exists = File.Exists(outputPath);
                    break;
                case AutoBuildPlatform.IOS:
                    exists = Directory.Exists(outputPath) &&
                             Directory.GetFiles(outputPath, "project.pbxproj", SearchOption.AllDirectories).Length > 0;
                    break;
                case AutoBuildPlatform.WebGL:
                    exists = File.Exists(Path.Combine(outputPath, "index.html"));
                    break;
                default:
                    exists = false;
                    break;
            }

            if (!exists) throw new IOException($"Unity 报告成功但未找到预期产物：{outputPath}");
            return outputPath;
        }

        /// <summary>
        /// 清理不适用于产物文件名的字符
        /// </summary>
        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Game";
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value;
        }
    }

    /// <summary>
    /// 自动构建本机凭据键
    /// </summary>
    internal static class AutoBuildCredentialKeys
    {
        /// <summary>
        /// 生成档案下的凭据键
        /// </summary>
        public static string ForProfile(AutoBuildProfile profile, string name)
        {
            return $"{profile.profileId}:{name}";
        }
    }
}
#endif
