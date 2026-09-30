# Common

本目录收纳 UI 与特效可复用小组件、按钮扩展及翻译辅助。根目录含 `BaiduTranslate`（百度翻译 HTTP 请求封装，需自行提供有效 App ID/密钥并考虑网络/平台限制）、特效生命周期脚本、网格残影 `GhostTrail`、文字走马灯脚本及“走马灯效果”示例 Prefab。部分历史类名使用 `Destory` 拼写，引用时以源码类型名为准。

## 按钮子模块

- `Btn`：`BtnExtension` 扩展 Unity Button 的双击/按压交互，`TemplateBtn` 提供点击、双击、长按回调，`YangToggleGroup` 管理带索引回调的 Toggle 集合；`Editor/BtnExtensionEditor` 提供对应 Inspector 编辑扩展。
- `CommonBtn`：`UICustomButton`、`UICustomSlider`、`UICustomToggle` 为通用 UI 控件封装。

常见接入是在 UI 物体上添加对应 Button/Toggle 及扩展组件，并在 Inspector 或代码中绑定事件：

```csharp
templateBtn.SetClick(() => OpenPanel(), () => ClosePanel());
toggleGroup.SetAction(index => SelectTab(index));
```

`BtnExtensionEditor` 只影响编辑器 Inspector，不是运行时组件。特效销毁组件通常由 Prefab 生命周期驱动；需要循环播放的效果应显式调用其停止/销毁入口，并避免重复创建协程或残影对象。
