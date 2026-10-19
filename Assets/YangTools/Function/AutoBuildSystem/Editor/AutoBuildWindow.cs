#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 自动构建档案配置和执行面板
    /// </summary>
    public sealed class AutoBuildWindow : EditorWindow
    {
        private const string ProfileFolder = "Assets/YangTools/Function/AutoBuildSystem/Profiles";
        private static readonly string[] TabNames = { "基本", "平台", "YooAsset", "定时", "邮件", "历史" };
        private readonly List<AutoBuildProfile> profiles = new List<AutoBuildProfile>();
        private Vector2 scrollPosition;
        private int selectedIndex;
        private int selectedTab;
        private string statusMessage = string.Empty;
        private string[] validationErrors = Array.Empty<string>();
        private string keystorePassword = string.Empty;
        private string aliasPassword = string.Empty;
        private string smtpPassword = string.Empty;

        #region 窗口生命周期

        /// <summary>
        /// 打开自动构建配置窗口
        /// </summary>
        [MenuItem("YangTools/自动构建/配置与运行", priority = 100)]
        public static void Open()
        {
            GetWindow<AutoBuildWindow>("自动构建").Show();
        }

        /// <summary>
        /// 初始化档案列表和本机凭据
        /// </summary>
        private void OnEnable()
        {
            minSize = new Vector2(610, 520);
            RefreshProfiles();
        }

        /// <summary>
        /// 绘制自动构建配置和操作控件
        /// </summary>
        private void OnGUI()
        {
            DrawProfileToolbar();
            AutoBuildProfile profile = GetSelectedProfile();
            if (profile == null)
            {
                EditorGUILayout.HelpBox("请新建一个构建档案，或选择已有档案。", MessageType.Info);
                return;
            }

            profile.EnsureDefaults();
            EditorGUI.BeginChangeCheck();
            selectedTab = GUILayout.Toolbar(selectedTab, TabNames);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            switch (selectedTab)
            {
                case 0: DrawGeneral(profile); break;
                case 1: DrawPlatforms(profile); break;
                case 2: DrawBundles(profile); break;
                case 3: DrawSchedule(profile); break;
                case 4: DrawEmail(profile); break;
                case 5: DrawHistory(); break;
            }
            EditorGUILayout.EndScrollView();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(profile);
                validationErrors = Array.Empty<string>();
            }

            DrawActions(profile);
        }

        #endregion

        #region 配置面板绘制

        /// <summary>
        /// 绘制档案切换和管理工具栏
        /// </summary>
        private void DrawProfileToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (profiles.Count > 0)
            {
                string[] labels = profiles.Select(profile => profile.profileName).ToArray();
                int nextIndex = EditorGUILayout.Popup(selectedIndex, labels, EditorStyles.toolbarPopup);
                if (nextIndex != selectedIndex)
                {
                    SaveSecrets(false);
                    selectedIndex = nextIndex;
                    LoadSecrets(GetSelectedProfile());
                    validationErrors = Array.Empty<string>();
                }
            }
            else
            {
                GUILayout.Label("尚无构建档案", EditorStyles.miniLabel);
            }

            if (GUILayout.Button("新建", EditorStyles.toolbarButton, GUILayout.Width(48))) CreateProfile();
            GUI.enabled = GetSelectedProfile() != null;
            if (GUILayout.Button("复制", EditorStyles.toolbarButton, GUILayout.Width(48))) DuplicateProfile();
            if (GUILayout.Button("删除", EditorStyles.toolbarButton, GUILayout.Width(48))) DeleteProfile();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(AutoBuildRunner.IsRunning
                ? "构建中：" + AutoBuildRunner.CurrentStatus
                : "定时任务仅在 Unity Editor 开启且电脑唤醒时运行。错过的任务不会补跑。", MessageType.None);
        }

        /// <summary>
        /// 绘制通用产品信息和输出目录
        /// </summary>
        private void DrawGeneral(AutoBuildProfile profile)
        {
            EditorGUILayout.LabelField("应用信息", EditorStyles.boldLabel);
            profile.profileName = EditorGUILayout.TextField("档案名称", profile.profileName);
            profile.companyName = EditorGUILayout.TextField("公司名称", profile.companyName);
            profile.productName = EditorGUILayout.TextField("产品名称", profile.productName);
            profile.version = EditorGUILayout.TextField("版本号", profile.version);
            profile.buildNumberCode = EditorGUILayout.IntField("构建号", profile.buildNumberCode);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("输出", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            profile.outputRoot = EditorGUILayout.TextField("输出根目录", profile.outputRoot);
            if (GUILayout.Button("选择", GUILayout.Width(55)))
            {
                string selected = EditorUtility.OpenFolderPanel("选择构建输出根目录", profile.outputRoot, string.Empty);
                if (!string.IsNullOrEmpty(selected)) profile.outputRoot = selected;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("相对路径以 Unity 项目根目录为基准；每次构建写入独立的档案与时间戳目录。", MessageType.Info);
        }

        /// <summary>
        /// 绘制各平台包标识图标和 Android 签名设置
        /// </summary>
        private void DrawPlatforms(AutoBuildProfile profile)
        {
            foreach (AutoBuildPlatform platform in Enum.GetValues(typeof(AutoBuildPlatform)))
            {
                AutoBuildPlatformSettings settings = profile.GetPlatform(platform);
                if (settings == null) continue;
                EditorGUILayout.BeginVertical("box");
                settings.enabled = EditorGUILayout.ToggleLeft(AutoBuildProfile.GetPlatformDisplayName(platform), settings.enabled, EditorStyles.boldLabel);
                if (settings.enabled)
                {
                    settings.icon = (Texture2D)EditorGUILayout.ObjectField("应用图标", settings.icon, typeof(Texture2D), false);
                    settings.applicationIdentifier = EditorGUILayout.TextField(
                        platform == AutoBuildPlatform.Android || platform == AutoBuildPlatform.IOS ? "包标识" : "应用标识",
                        settings.applicationIdentifier);

                    if (platform == AutoBuildPlatform.Android)
                    {
                        settings.androidFormat = (AutoBuildAndroidFormat)EditorGUILayout.EnumPopup("产物格式", settings.androidFormat);
                        settings.useCustomKeystore = EditorGUILayout.Toggle("使用自定义 Keystore", settings.useCustomKeystore);
                        if (settings.useCustomKeystore)
                        {
                            DrawKeystorePath(settings);
                            settings.keyAlias = EditorGUILayout.TextField("Key Alias", settings.keyAlias);
                            DrawCredentialField("Keystore 密码", ref keystorePassword);
                            DrawCredentialField("Alias 密码", ref aliasPassword);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.HelpBox("Windows 运行环境下的 iOS 构建结果是 Xcode 工程；最终 IPA 归档和签名需在 macOS/Xcode 完成。", MessageType.Warning);
        }

        /// <summary>
        /// 绘制 Keystore 路径选择器
        /// </summary>
        private static void DrawKeystorePath(AutoBuildPlatformSettings settings)
        {
            EditorGUILayout.BeginHorizontal();
            settings.keystorePath = EditorGUILayout.TextField("Keystore 文件", settings.keystorePath);
            if (GUILayout.Button("浏览", GUILayout.Width(55)))
            {
                string directory = string.IsNullOrEmpty(settings.keystorePath) ? string.Empty : Path.GetDirectoryName(settings.keystorePath);
                string selected = EditorUtility.OpenFilePanel("选择 Android Keystore", directory, "keystore");
                if (!string.IsNullOrEmpty(selected)) settings.keystorePath = selected;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制 YooAsset 构建参数
        /// </summary>
        private static void DrawBundles(AutoBuildProfile profile)
        {
            AutoBuildBundleSettings settings = profile.bundles;
            settings.enabled = EditorGUILayout.ToggleLeft("构建 YooAsset 资源包", settings.enabled, EditorStyles.boldLabel);
            if (!settings.enabled) return;

            settings.packageName = EditorGUILayout.TextField("YooAsset 包名", settings.packageName);
            settings.packageVersion = EditorGUILayout.TextField("包版本", settings.packageVersion);
            settings.pipeline = (YooAsset.Editor.EBuildPipeline)EditorGUILayout.EnumPopup("构建管线", settings.pipeline);
            settings.compression = (YooAsset.Editor.ECompressOption)EditorGUILayout.EnumPopup("压缩方式", settings.compression);
            settings.fileNameStyle = (EFileNameStyle)EditorGUILayout.EnumPopup("文件名样式", settings.fileNameStyle);
            settings.buildinFileCopyOption = (YooAsset.Editor.EBuildinFileCopyOption)EditorGUILayout.EnumPopup("内置文件拷贝", settings.buildinFileCopyOption);
            if (settings.buildinFileCopyOption == YooAsset.Editor.EBuildinFileCopyOption.ClearAndCopyByTags ||
                settings.buildinFileCopyOption == YooAsset.Editor.EBuildinFileCopyOption.OnlyCopyByTags)
            {
                settings.buildinFileCopyParams = EditorGUILayout.TextField("拷贝标签", settings.buildinFileCopyParams);
            }
            settings.clearBuildCacheFiles = EditorGUILayout.Toggle("清理增量缓存", settings.clearBuildCacheFiles);
            settings.verifyBuildingResult = EditorGUILayout.Toggle("构建后校验", settings.verifyBuildingResult);
            settings.useAssetDependencyDB = EditorGUILayout.Toggle("使用依赖缓存数据库", settings.useAssetDependencyDB);
        }

        /// <summary>
        /// 绘制定时构建配置
        /// </summary>
        private static void DrawSchedule(AutoBuildProfile profile)
        {
            AutoBuildScheduleSettings settings = profile.schedule;
            settings.enabled = EditorGUILayout.ToggleLeft("启用定时构建", settings.enabled, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!settings.enabled))
            {
                settings.mode = (AutoBuildScheduleMode)EditorGUILayout.EnumPopup("周期", settings.mode);
                settings.hour = EditorGUILayout.IntSlider("小时", settings.hour, 0, 23);
                settings.minute = EditorGUILayout.IntSlider("分钟", settings.minute, 0, 59);
                if (settings.mode == AutoBuildScheduleMode.Weekly)
                {
                    settings.dayOfWeek = (DayOfWeek)EditorGUILayout.EnumPopup("星期", settings.dayOfWeek);
                }
            }

            EditorGUILayout.HelpBox("使用本机时区。编辑器关闭或电脑休眠时不会运行；计划恢复后跳过过期任务，不自动补跑。", MessageType.Info);
        }

        /// <summary>
        /// 绘制 SMTP 配置和本机凭据输入
        /// </summary>
        private void DrawEmail(AutoBuildProfile profile)
        {
            AutoBuildEmailSettings settings = profile.email;
            settings.enabled = EditorGUILayout.ToggleLeft("构建完成或失败后发送邮件", settings.enabled, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!settings.enabled))
            {
                settings.host = EditorGUILayout.TextField("SMTP 服务器", settings.host);
                settings.port = EditorGUILayout.IntField("端口", settings.port);
                settings.sender = EditorGUILayout.TextField("发件人", settings.sender);
                settings.recipient = EditorGUILayout.TextField("收件人 (逗号分隔)", settings.recipient);
                settings.username = EditorGUILayout.TextField("SMTP 用户名", settings.username);
                settings.enableTls = EditorGUILayout.Toggle("启用 TLS", settings.enableTls);
                DrawCredentialField("SMTP 密码", ref smtpPassword);
                EditorGUILayout.HelpBox("密码保存在用户本机 DPAPI 凭据文件中，不会写入档案资产或构建日志。", MessageType.Info);
            }
        }

        /// <summary>
        /// 显示最近的构建结果和日志位置
        /// </summary>
        private void DrawHistory()
        {
            List<AutoBuildBuildRecord> records = AutoBuildHistoryStore.LoadRecent(20);
            if (records.Count == 0)
            {
                EditorGUILayout.HelpBox("尚无构建记录。", MessageType.Info);
                return;
            }

            foreach (AutoBuildBuildRecord record in records)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"{record.finishedAt}  {record.profileName}  {record.status}  {record.durationSeconds:0.0} 秒", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(record.message, EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrEmpty(record.logPath))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel(record.logPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    if (File.Exists(record.logPath) && GUILayout.Button("打开日志", GUILayout.Width(72)))
                    {
                        EditorUtility.OpenWithDefaultApp(record.logPath);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 绘制凭据加密保存操作
        /// </summary>
        private void DrawCredentialField(string label, ref string value)
        {
            value = EditorGUILayout.PasswordField(label, value);
            if (GUILayout.Button("保存本机凭据", GUILayout.Width(110))) SaveSecrets(true);
        }

        /// <summary>
        /// 绘制校验、构建、邮件测试和保存按钮
        /// </summary>
        private void DrawActions(AutoBuildProfile profile)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存配置")) SaveProfile(profile);
            if (GUILayout.Button("校验配置"))
            {
                validationErrors = AutoBuildProfileValidator.Validate(profile).ToArray();
                statusMessage = validationErrors.Length == 0 ? "档案校验通过" : "档案存在错误";
            }

            using (new EditorGUI.DisabledScope(AutoBuildRunner.IsRunning))
            {
                if (GUILayout.Button("立即构建"))
                {
                    SaveProfile(profile);
                    if (AutoBuildRunner.TryStart(profile, false, out string error)) statusMessage = "构建已启动";
                    else statusMessage = error;
                }

                using (new EditorGUI.DisabledScope(profile.email == null || !profile.email.enabled))
                {
                    if (GUILayout.Button("测试邮件")) TestEmail(profile);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(statusMessage)) EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            foreach (string error in validationErrors) EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        #endregion

        #region 档案与凭据管理

        /// <summary>
        /// 刷新档案列表并保持当前选择
        /// </summary>
        private void RefreshProfiles()
        {
            string previousPath = GetSelectedProfile() == null ? string.Empty : AssetDatabase.GetAssetPath(GetSelectedProfile());
            profiles.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:AutoBuildProfile", new[] { ProfileFolder }))
            {
                AutoBuildProfile profile = AssetDatabase.LoadAssetAtPath<AutoBuildProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if (profile != null) profiles.Add(profile);
            }
            profiles.Sort((left, right) => string.Compare(left.profileName, right.profileName, StringComparison.OrdinalIgnoreCase));
            selectedIndex = Math.Max(0, profiles.FindIndex(profile => AssetDatabase.GetAssetPath(profile) == previousPath));
            LoadSecrets(GetSelectedProfile());
        }

        /// <summary>
        /// 获取当前选中的档案
        /// </summary>
        private AutoBuildProfile GetSelectedProfile()
        {
            return selectedIndex >= 0 && selectedIndex < profiles.Count ? profiles[selectedIndex] : null;
        }

        /// <summary>
        /// 创建一个未启用定时任务的新档案
        /// </summary>
        private void CreateProfile()
        {
            EnsureProfileFolder();
            AutoBuildProfile profile = CreateInstance<AutoBuildProfile>();
            profile.profileId = Guid.NewGuid().ToString("N");
            profile.profileName = "构建档案";
            profile.EnsureDefaults();
            string path = AssetDatabase.GenerateUniqueAssetPath(ProfileFolder + "/AutoBuildProfile.asset");
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            RefreshProfiles();
            selectedIndex = profiles.FindIndex(item => AssetDatabase.GetAssetPath(item) == path);
            LoadSecrets(GetSelectedProfile());
        }

        /// <summary>
        /// 复制当前档案并生成独立凭据标识
        /// </summary>
        private void DuplicateProfile()
        {
            AutoBuildProfile source = GetSelectedProfile();
            if (source == null) return;
            EnsureProfileFolder();
            AutoBuildProfile clone = CreateInstance<AutoBuildProfile>();
            EditorUtility.CopySerialized(source, clone);
            clone.profileId = Guid.NewGuid().ToString("N");
            clone.profileName = source.profileName + "_副本";
            clone.schedule.enabled = false;
            string path = AssetDatabase.GenerateUniqueAssetPath(ProfileFolder + "/" + MakeSafeFileName(clone.profileName) + ".asset");
            AssetDatabase.CreateAsset(clone, path);
            AssetDatabase.SaveAssets();
            RefreshProfiles();
            selectedIndex = profiles.FindIndex(item => AssetDatabase.GetAssetPath(item) == path);
            LoadSecrets(GetSelectedProfile());
        }

        /// <summary>
        /// 删除档案并清理该档案关联的本机凭据
        /// </summary>
        private void DeleteProfile()
        {
            AutoBuildProfile profile = GetSelectedProfile();
            if (profile == null || !EditorUtility.DisplayDialog("删除构建档案", $"确认删除 {profile.profileName}？", "删除", "取消")) return;
            SaveSecrets(false);
            AutoBuildSecretStore.Set(AutoBuildCredentialKeys.ForProfile(profile, "android.keystore"), string.Empty);
            AutoBuildSecretStore.Set(AutoBuildCredentialKeys.ForProfile(profile, "android.alias"), string.Empty);
            AutoBuildSecretStore.Set(AutoBuildCredentialKeys.ForProfile(profile, "smtp.password"), string.Empty);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(profile));
            AssetDatabase.SaveAssets();
            RefreshProfiles();
            AutoBuildScheduler.Refresh();
        }

        /// <summary>
        /// 保存档案资产和定时器配置
        /// </summary>
        private void SaveProfile(AutoBuildProfile profile)
        {
            profile.EnsureDefaults();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AutoBuildScheduler.Refresh();
            statusMessage = "构建档案已保存";
        }

        /// <summary>
        /// 保存当前窗口中的密码到 DPAPI 本机凭据文件
        /// </summary>
        private void SaveSecrets(bool showMessage)
        {
            AutoBuildProfile profile = GetSelectedProfile();
            if (profile == null) return;
            try
            {
                AutoBuildSecretStore.Set(AutoBuildCredentialKeys.ForProfile(profile, "android.keystore"), keystorePassword);
                AutoBuildSecretStore.Set(AutoBuildCredentialKeys.ForProfile(profile, "android.alias"), aliasPassword);
                AutoBuildSecretStore.Set(AutoBuildCredentialKeys.ForProfile(profile, "smtp.password"), smtpPassword);
                if (showMessage) statusMessage = "本机凭据已加密保存";
            }
            catch (Exception exception)
            {
                statusMessage = "凭据保存失败：" + exception.Message;
            }
        }

        /// <summary>
        /// 读取选中档案对应的本机凭据到密码输入框
        /// </summary>
        private void LoadSecrets(AutoBuildProfile profile)
        {
            keystorePassword = string.Empty;
            aliasPassword = string.Empty;
            smtpPassword = string.Empty;
            if (profile == null) return;

            try
            {
                keystorePassword = AutoBuildSecretStore.Get(AutoBuildCredentialKeys.ForProfile(profile, "android.keystore"));
                aliasPassword = AutoBuildSecretStore.Get(AutoBuildCredentialKeys.ForProfile(profile, "android.alias"));
                smtpPassword = AutoBuildSecretStore.Get(AutoBuildCredentialKeys.ForProfile(profile, "smtp.password"));
            }
            catch (Exception exception)
            {
                statusMessage = "本机凭据读取失败：" + exception.Message;
            }
        }

        /// <summary>
        /// 发送测试邮件并显示实际发送结果
        /// </summary>
        private async void TestEmail(AutoBuildProfile profile)
        {
            SaveProfile(profile);
            SaveSecrets(false);
            statusMessage = "正在发送测试邮件";
            try
            {
                await AutoBuildNotifier.SendTestAsync(profile);
                statusMessage = "测试邮件已发送";
            }
            catch (Exception exception)
            {
                statusMessage = "测试邮件失败：" + exception.Message;
            }
            Repaint();
        }

        /// <summary>
        /// 确保档案目录在 Assets 下存在
        /// </summary>
        private static void EnsureProfileFolder()
        {
            string[] folders = { "Assets/YangTools", "Assets/YangTools/Function", "Assets/YangTools/Function/AutoBuildSystem", ProfileFolder };
            for (int i = 0; i < folders.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(folders[i])) continue;
                int separator = folders[i].LastIndexOf('/');
                AssetDatabase.CreateFolder(folders[i].Substring(0, separator), folders[i].Substring(separator + 1));
            }
        }

        /// <summary>
        /// 生成合法的 Unity 资产文件名
        /// </summary>
        private static string MakeSafeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return string.IsNullOrWhiteSpace(value) ? "AutoBuildProfile" : value;
        }

        /// <summary>
        /// 清空窗口内的密码缓存
        /// </summary>
        private void OnDisable()
        {
            SaveSecrets(false);
            keystorePassword = string.Empty;
            aliasPassword = string.Empty;
            smtpPassword = string.Empty;
        }

        #endregion
    }
}
#endif
