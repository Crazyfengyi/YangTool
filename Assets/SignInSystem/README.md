# 独立签到系统

此目录为七日签到界面、预制体和美术资源，独立模块

## 模块依赖

- Unity UI（UGUI）：需要启用 Unity UI 模块（`com.unity.modules.ui` 或 Unity 6 中对应的 UGUI 包）
- TextMeshPro：需要安装 `com.unity.textmeshpro` 包
- 模块程序集定义为 `SignInSystem.asmdef`

导出时请同时保留本目录下的 `.meta` 文件，并在目标项目中安装上述 Unity 包。签到系统使用 Unity 原生 `Button` 和 TextMeshPro UI 组件。

模块内 UI 辅助类和数据项统一位于 `SignInSystem` 命名空间。`SignInManager`、`SignInConfig` 和 `SevenSignWindow` 保持原有全局类型名，以兼容已有预制体和宿主项目调用。

## 目录结构

- `Scripts`：签到管理器、窗口和奖励项脚本
- `Prefabs`：签到窗口及奖励项预制体
- `Resources/SignInConfig.asset`：七日签到配置
- `Textures`：签到界面图片
- `Fonts`：签到界面字体资源

## 快速使用

1. `Prefabs/SignInManager.prefab`管理器预制体单例放进场景,将 `Prefabs/SevenSignWindow.prefab` 放到 Canvas 下
2. 确认场景中存在 `EventSystem`
3. 运行场景，窗口会自动读取 `Resources/SignInConfig.asset`
4. 在业务代码中订阅 `SignInManager.Instance.SignedIn`，根据天数、倍率和奖励道具信息发放实际奖励

```csharp
using System.Collections.Generic;

private void OnEnable()
{
    SignInManager.Instance.SignedIn += OnSignedIn;
}

private void OnDisable()
{
    SignInManager.Instance.SignedIn -= OnSignedIn;
}

private void OnSignedIn(int dayIndex, int multiplier, IReadOnlyList<SignInReward> rewards)
{
    // 根据配置发放游戏内奖励
}
```

## 接入外部签到数据

其他项目可以在运行时将自己的数据转换为 `SignInDayData` 和 `SignInReward` 后注入：

```csharp
using System.Collections.Generic;

List<SignInDayData> projectDays = LoadProjectSignInDays();
SignInManager.Instance.InjectSignInData(projectDays);
```

注入数据的优先级高于 `SignInConfig`，支持在运行中重复注入以切换账号或活动。系统会复制注入的天数和奖励对象，外部列表后续修改不会影响签到系统。

如果传入 `null` 或空列表，注入数据会被清除并恢复管理器预制体上的 `SignInConfig`；未配置预制体引用时继续读取 `Resources/SignInConfig.asset`，两者都缺失时使用内置七日默认数据。

## 配置与存档

直接编辑 `Resources/SignInConfig.asset` 可修改签到天数、奖励 ID、数量和图标
签到进度保存在 `PlayerPrefs`，键名前缀为 `SignInSystem.`

测试时可调用 `SignInManager.Instance.ResetProgress()` 清除本地进度。

## 相关预制体

`ItemUI_SignAward-01.prefab` 和 `ItemUI_BagProp_Sign.prefab` 是签到奖励项预制体，`Textures` 和 `Fonts` 保存界面依赖的美术资源。
