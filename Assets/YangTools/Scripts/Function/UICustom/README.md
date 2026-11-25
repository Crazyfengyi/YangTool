# UICustom UGUI 扩展

## 功能与入口

| 组件 | 内容 |
| --- | --- |
| [CustomScrollRect](CustomScrollRect.cs) | 将与自身主滚动方向不同的拖拽或滚轮事件转交父 ScrollRect |
| [RoundCornerRectImage](RoundCornerRectImage.cs) | 通过 UI 顶点生成圆角矩形及可选描边 |
| [EffectCanvas](EffectCanvas.cs) | Graphic 派生类型的空入口 |

## 依赖与配置

依赖 UGUI 与 EventSystem。嵌套滚动需要父 ScrollRect，子区域配置单一主方向；圆角组件使用 RectTransform 尺寸、圆角半径与分段数。

## 最小接入

在嵌套列表的子滚动区域使用 CustomScrollRect 并保留原 Content、Viewport 等绑定；或在 Canvas 下添加 RoundCornerRectImage 配置圆角与描边。

## 示例与限制

本目录没有演示场景。子滚动同时启用横纵方向时不转交父级；运行中换父节点需留意父引用刷新时机。EffectCanvas 当前没有额外的网格或着色器实现，添加组件不会自动产生特效。材质仍需与 Canvas 和 UI 遮罩兼容。
