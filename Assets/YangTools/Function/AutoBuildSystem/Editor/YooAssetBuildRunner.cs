#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 通过 YooAsset 已有构建管线生成资源包
    /// </summary>
    internal static class YooAssetBuildRunner
    {
        /// <summary>
        /// 按档案配置构建指定平台资源包
        /// </summary>
        public static string Build(AutoBuildProfile profile, AutoBuildPlatform platform, string outputRoot, string runId)
        {
            AutoBuildBundleSettings settings = profile.bundles;
            BuildTarget target = AutoBuildProfile.ToBuildTarget(platform);
            BuildParametersBase parameters;
            IBuildPipeline pipeline;

            if (settings.pipeline == EBuildPipeline.ScriptableBuildPipeline)
            {
                ScriptableBuildParameters scriptableParameters = CreateScriptableParameters(profile, target, outputRoot, runId);
                parameters = new BuildParametersBase(scriptableParameters);
                pipeline = new ScriptableBuildPipeline();
            }
            else
            {
                BuiltinBuildParameters builtinParameters = CreateBuiltinParameters(profile, target, outputRoot, runId);
                parameters = new BuildParametersBase(builtinParameters);
                pipeline = new BuiltinBuildPipeline();
            }

            YooAssetBuildResult result = parameters.Run(pipeline);
            if (!result.success)
            {
                throw new InvalidOperationException($"YooAsset 资源包构建失败：{result.error}");
            }

            if (string.IsNullOrWhiteSpace(result.outputPath) || !Directory.Exists(result.outputPath))
            {
                throw new IOException("YooAsset 报告构建成功但输出目录不存在");
            }

            return result.outputPath;
        }

        private static ScriptableBuildParameters CreateScriptableParameters(AutoBuildProfile profile, BuildTarget target,
            string outputRoot, string runId)
        {
            return new ScriptableBuildParameters
            {
                BuildOutputRoot = outputRoot,
                BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),
                BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString(),
                BuildBundleType = (int)EBuildBundleType.AssetBundle,
                BuildTarget = target,
                PackageName = profile.bundles.packageName,
                PackageVersion = profile.bundles.packageVersion,
                PackageNote = runId,
                EnableSharePackRule = true,
                VerifyBuildingResult = profile.bundles.verifyBuildingResult,
                FileNameStyle = profile.bundles.fileNameStyle,
                BuildinFileCopyOption = profile.bundles.buildinFileCopyOption,
                BuildinFileCopyParams = profile.bundles.buildinFileCopyParams ?? string.Empty,
                CompressOption = profile.bundles.compression,
                ClearBuildCacheFiles = profile.bundles.clearBuildCacheFiles,
                UseAssetDependencyDB = profile.bundles.useAssetDependencyDB,
                BuiltinShadersBundleName = GetBuiltinShaderBundleName(profile.bundles.packageName)
            };
        }

        private static BuiltinBuildParameters CreateBuiltinParameters(AutoBuildProfile profile, BuildTarget target,
            string outputRoot, string runId)
        {
            return new BuiltinBuildParameters
            {
                BuildOutputRoot = outputRoot,
                BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),
                BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString(),
                BuildBundleType = (int)EBuildBundleType.AssetBundle,
                BuildTarget = target,
                PackageName = profile.bundles.packageName,
                PackageVersion = profile.bundles.packageVersion,
                PackageNote = runId,
                EnableSharePackRule = true,
                VerifyBuildingResult = profile.bundles.verifyBuildingResult,
                FileNameStyle = profile.bundles.fileNameStyle,
                BuildinFileCopyOption = profile.bundles.buildinFileCopyOption,
                BuildinFileCopyParams = profile.bundles.buildinFileCopyParams ?? string.Empty,
                CompressOption = profile.bundles.compression,
                ClearBuildCacheFiles = profile.bundles.clearBuildCacheFiles,
                UseAssetDependencyDB = profile.bundles.useAssetDependencyDB
            };
        }

        private static string GetBuiltinShaderBundleName(string packageName)
        {
            bool uniqueNames = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            return DefaultPackRule.CreateShadersPackRuleResult().GetBundleName(packageName, uniqueNames);
        }

        private sealed class BuildParametersBase
        {
            private readonly YooAsset.Editor.BuildParameters parameters;

            public BuildParametersBase(YooAsset.Editor.BuildParameters parameters)
            {
                this.parameters = parameters;
            }

            public YooAssetBuildResult Run(IBuildPipeline pipeline)
            {
                YooAsset.Editor.BuildResult result = pipeline.Run(parameters, true);
                return new YooAssetBuildResult
                {
                    success = result.Success,
                    error = string.IsNullOrWhiteSpace(result.ErrorInfo) ? result.FailedTask : result.ErrorInfo,
                    outputPath = result.OutputPackageDirectory
                };
            }
        }

        private sealed class YooAssetBuildResult
        {
            public bool success;
            public string error;
            public string outputPath;
        }
    }
}
#endif
