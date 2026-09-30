# UICustom

该目录提供 UGUI 控件扩展：`CustomScrollRect` 扩展滚动区域行为，`EffectCanvas` 是用于自定义 UI 图形/材质效果的 Graphic 基类，`RoundCornerRectImage` 提供圆角矩形 Image。

在 Canvas 下按控件用途添加对应组件；`RoundCornerRectImage` 需配置 Image 所需的 Sprite/材质及尺寸，效果边界受 RectTransform 和材质参数影响。自定义 Graphic 若需特殊 Shader/材质，应确保 Shader 被项目打包且 Canvas 渲染模式兼容。目录没有额外自动注册步骤。
