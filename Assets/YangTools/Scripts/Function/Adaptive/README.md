# Adaptive

`TextAdaptive` 根据文本首选尺寸缩小 Unity UGUI `Text` 的字号，使内容适配自身 `RectTransform`。组件位于 [TextAdaptive.cs](TextAdaptive.cs)，并要求同物体上有 `UnityEngine.UI.Text`。

在 Inspector 添加组件，选择横向或纵向适配，并保证文本区域尺寸已布局完成。默认纵向适配；`isReuse` 适用于运行时会反复改字的文本。脚本会在 LateUpdate 测量并缩小字号，不会自动放大。中文相邻半角空格会替换为不换行空格；需要保留这类空格行为时请留意。

```csharp
text.text = "任务已完成";
textAdaptive.OnTextChange(); // 需要时可显式标记重新适配
```

普通使用由 `Text` 的顶点变化回调触发重新测量；避免同时启用 Text 自带的 Best Fit（组件会关闭该设置）。
