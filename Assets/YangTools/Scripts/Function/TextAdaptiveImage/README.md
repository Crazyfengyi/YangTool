# TextAdaptiveImage 文本布局最大宽度

## 功能与入口

[TextAdaptiveImage.cs](TextAdaptiveImage.cs) 在 TMP 文本顶点变化后检查首选宽度，达到上限时将 `LayoutElement.preferredWidth` 设为 `maxWidth`，否则设为 -1 交回布局系统。

## 依赖与配置

同物体需要 `TextMeshProUGUI` 和 `LayoutElement`。父级应使用能读取布局元素的 LayoutGroup 或相关布局结构，Inspector 配置正数 `maxWidth`。

## 最小接入

在目标 TMP 物体上添加组件和 LayoutElement，设置最大宽度后更新文本即可。该脚本通过文本顶点变化回调刷新，不需要额外业务调用。

## 示例与限制

[Canvas.prefab](Canvas.prefab) 为布局参考。组件限制的是布局首选宽度，不直接修改字体大小，也不直接缩放背景图片。换行、高度和背景自适应仍取决于 TMP、父级布局与 ContentSizeFitter 配置。
