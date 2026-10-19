#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using YooAsset.Editor;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 自动构建档案校验器
    /// </summary>
    public static class AutoBuildProfileValidator
    {
        private static readonly Regex IdentifierPattern = new Regex(@"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$", RegexOptions.Compiled);

        /// <summary>
        /// 返回档案中阻止构建的配置错误
        /// </summary>
        public static List<string> Validate(AutoBuildProfile profile)
        {
            List<string> errors = new List<string>();
            if (profile == null)
            {
                errors.Add("未选择构建档案");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(profile.profileName)) errors.Add("档案名称不能为空");
            if (string.IsNullOrWhiteSpace(profile.productName)) errors.Add("产品名称不能为空");
            if (!Version.TryParse(profile.version, out _)) errors.Add("版本号需使用 x.y 或 x.y.z 格式");
            if (string.IsNullOrWhiteSpace(profile.outputRoot)) errors.Add("构建输出目录不能为空");
            if (profile.platforms == null || profile.platforms.FindAll(item => item != null && item.enabled).Count == 0)
                errors.Add("至少启用一个 Player 构建平台");

            if (profile.buildNumberCode < 1) errors.Add("构建号必须大于 0");

            if (profile.platforms != null)
            {
                foreach (AutoBuildPlatformSettings settings in profile.platforms)
                {
                    if (settings == null || !settings.enabled) continue;
                    ValidatePlatform(profile, settings, errors);
                }
            }

            ValidateBundles(profile, errors);
            ValidateEmail(profile, errors);
            return errors;
        }

        /// <summary>
        /// 校验指定平台的必要参数
        /// </summary>
        private static void ValidatePlatform(AutoBuildProfile profile, AutoBuildPlatformSettings settings, List<string> errors)
        {
            bool requiresIdentifier = settings.platform == AutoBuildPlatform.Android || settings.platform == AutoBuildPlatform.IOS;
            if ((requiresIdentifier || !string.IsNullOrWhiteSpace(settings.applicationIdentifier)) &&
                !IdentifierPattern.IsMatch(settings.applicationIdentifier ?? string.Empty))
            {
                string identifierLabel = requiresIdentifier ? "包标识" : "应用标识";
                errors.Add($"{AutoBuildProfile.GetPlatformDisplayName(settings.platform)} {identifierLabel}格式不正确");
            }

            if (settings.platform == AutoBuildPlatform.Android && settings.useCustomKeystore)
            {
                if (string.IsNullOrWhiteSpace(settings.keystorePath) || !File.Exists(settings.keystorePath))
                    errors.Add("Android Keystore 文件不存在");
                if (string.IsNullOrWhiteSpace(settings.keyAlias)) errors.Add("Android Key Alias 不能为空");
                if (!HasCredential(profile, "android.keystore", errors, "Android Keystore 密码"))
                    errors.Add("Android Keystore 密码尚未保存在本机凭据中");
                if (!HasCredential(profile, "android.alias", errors, "Android Alias 密码"))
                    errors.Add("Android Alias 密码尚未保存在本机凭据中");
            }
        }

        /// <summary>
        /// 校验 YooAsset 包配置
        /// </summary>
        private static void ValidateBundles(AutoBuildProfile profile, List<string> errors)
        {
            if (profile.bundles == null || !profile.bundles.enabled) return;
            if (string.IsNullOrWhiteSpace(profile.bundles.packageName))
            {
                errors.Add("YooAsset 包名不能为空");
                return;
            }

            if (string.IsNullOrWhiteSpace(profile.bundles.packageVersion)) errors.Add("YooAsset 包版本不能为空");
            if (profile.bundles.pipeline != EBuildPipeline.ScriptableBuildPipeline &&
                profile.bundles.pipeline != EBuildPipeline.BuiltinBuildPipeline)
            {
                errors.Add("自动构建仅支持 YooAsset SBP 或 BBP 资源管线");
            }

            bool packageExists = AssetBundleCollectorSettingData.Setting.Packages.Exists(
                package => package != null && package.PackageName == profile.bundles.packageName);
            if (!packageExists) errors.Add($"YooAsset 收集配置中不存在包：{profile.bundles.packageName}");
        }

        /// <summary>
        /// 校验邮件通知参数
        /// </summary>
        private static void ValidateEmail(AutoBuildProfile profile, List<string> errors)
        {
            if (profile.email == null || !profile.email.enabled) return;
            if (string.IsNullOrWhiteSpace(profile.email.host)) errors.Add("SMTP 服务器不能为空");
            if (profile.email.port < 1 || profile.email.port > 65535) errors.Add("SMTP 端口范围必须为 1 到 65535");
            if (string.IsNullOrWhiteSpace(profile.email.sender)) errors.Add("邮件发件人不能为空");
            if (string.IsNullOrWhiteSpace(profile.email.recipient)) errors.Add("邮件收件人不能为空");
            if (string.IsNullOrWhiteSpace(profile.email.username)) errors.Add("SMTP 用户名不能为空");
            if (!HasCredential(profile, "smtp.password", errors, "SMTP 密码"))
                errors.Add("SMTP 密码尚未保存在本机凭据中");
        }

        /// <summary>
        /// 检查本机凭据是否存在并转为可读错误
        /// </summary>
        private static bool HasCredential(AutoBuildProfile profile, string credentialName, List<string> errors, string displayName)
        {
            try
            {
                return !string.IsNullOrEmpty(AutoBuildSecretStore.Get(AutoBuildCredentialKeys.ForProfile(profile, credentialName)));
            }
            catch (Exception exception)
            {
                errors.Add($"无法读取{displayName}：{exception.Message}");
                return true;
            }
        }
    }
}
#endif
