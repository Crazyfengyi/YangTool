# 平台 SDK 适配

本目录将登录、广告、分享、震动、剪贴板、生命周期及平台 UI 能力抽象为 `IPlatform`，由 `PlatformMgr` 和平台子目录中的实现对接具体 SDK。

## 主要文件

- `IPlatform.cs`：跨平台业务接口。
- `PlatformMgr.cs`：场景平台适配组件，按 `UNITY_DOUYIN_GAME`、`UNITY_WECHAT_GAME` 等构建宏创建具体实现并转发调用。
- `Platform/Default`、`Platform/DouYin`、`Platform/WeiXin`：默认、抖音和微信平台实现。
- `StructDefine.cs`：平台类型、平台数据及相关事件数据。
- `RemoveUnityLoge.cs`：启动画面相关的小工具。

## 接入注意

在场景中放置并配置 `PlatformMgr`，填写平台类型和对应 `PlatformData`，再按目标平台设置构建宏并接入 SDK。`Awake` 会初始化具体平台；业务代码可通过 `PlatformMgr.Instance` 使用门面或 `PlatformMgr.Instance.Platform` 访问 `IPlatform`。不同平台的能力可能不同，调用前需处理回调中的成功/失败结果；具体配置以 `PlatformMgr.cs` 与 `Platform/` 实现为准。

```csharp
PlatformMgr.Instance.LookAd(success =>
{
    if (success) Debug.Log("广告流程完成");
}, "Reward");
```
