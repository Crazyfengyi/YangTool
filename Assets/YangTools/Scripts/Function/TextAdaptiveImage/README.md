# TextAdaptiveImage

本组件依据 TextMeshPro 文本首选宽度调整布局尺寸，适用于文本长度变化的 UI 标签。组件要求同一物体上有 `TextMeshProUGUI` 和 `LayoutElement`，并通过 `maxWidth` 限制最大宽度。

在 Inspector 设置 `maxWidth` 后更新 TMP 文本；组件会读取首选尺寸并更新布局。布局父级应使用兼容的 LayoutGroup/ContentSizeFitter 配置，避免多个布局控制器互相争写尺寸。超长文本的换行与高度行为受 TMP 宽度、换行设置和父布局影响。
