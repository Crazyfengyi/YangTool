# Adaptive 文本字号适配

## 功能与入口

[TextAdaptive.cs](TextAdaptive.cs) 根据 UGUI `Text` 的首选尺寸缩小字号，支持横向或纵向适配。默认纵向，不会主动把字号放大超过初始字号。

## 依赖与配置

同物体需要 `Text` 和有效尺寸的 `RectTransform`，Inspector 标签依赖 Odin。组件初始化会关闭 Text 的 Best Fit 并记录初始字号；`isReuse` 用于会反复修改内容的文本。

## 最小接入

在文本物体上添加 `TextAdaptive`，选择适配方向并设置文本区域。更新 `text.text` 后由顶点变化回调标记重新适配，也可调用 `textAdaptive.OnTextChange()`。测量和字号缩小发生在 `LateUpdate`。

## 示例与限制

本目录没有独立场景或 Prefab。仅适用于旧版 UGUI Text；TMP 的布局宽度限制参见 [TextAdaptiveImage](../TextAdaptiveImage/README.md)。中文相邻的半角空格会转换为不换行空格；最终字号还受最小字号和迭代次数约束。
