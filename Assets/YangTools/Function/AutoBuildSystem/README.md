# Unity 自动构建系统

## 打开配置面板

在 Unity 菜单中选择 **YangTools → 自动构建 → 配置与运行**，新建档案后配置目标平台、YooAsset、定时计划和邮件参数。可在面板中校验配置、立即构建和发送测试邮件。

## 运行约定

- 定时器由当前 Unity Editor 驱动，每日/每周按本机时间触发；Editor 更新间隔超过 5 秒或启动时发现过期任务，会记为跳过，不会补跑。编辑器关闭或电脑休眠期间不构建。
- 每次构建产物写入独立的档案与时间戳目录，日志和最近 50 条历史记录保存在用户本机 LocalAppData。
- 历史记录和完成/失败通知包含总耗时、输出路径；邮件附构建日志末尾摘要，完整日志路径可从面板打开。
- Keystore 与 SMTP 密码通过 Windows DPAPI 加密保存到用户本机，不进入 ScriptableObject、Git 或日志。每位开发者需要分别保存自己的凭据。
- AB 构建直接复用项目根目录的 YooAsset Collector Setting，首版支持 Scriptable Build Pipeline 和 Builtin Build Pipeline。
- iOS 目标在 Windows Editor 生成 Xcode 工程；最终 IPA 构建、签名和归档需在 macOS/Xcode 环境完成。
- 本模块与原有 `EditorStudio/Editor/AutoTools/BuildTool` 保持独立，不改动原构建工具及 YooAsset 收集规则。
