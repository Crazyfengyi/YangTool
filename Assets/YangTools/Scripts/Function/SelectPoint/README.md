# SelectPoint 网格拾取与吸附

## 功能与入口

[SceneEditor.cs](SceneEditor.cs) 演示顶点、三角面和共面区域拾取、高亮及两次选择后的吸附。[EditorHelper.cs](EditorHelper.cs) 提供射线、共面三角形集合和对齐计算。

## 依赖与配置

依赖 UGUI、主相机、可读网格与 MeshCollider。SceneEditor 直接使用 `Camera.main`，需给相机设置 MainCamera 标签；Inspector 绑定三种模式按钮及用于 GL 高亮的材质。要收到 OnPostRender，组件需挂在参与渲染的相机上，并核对渲染管线是否支持该回调。

## 最小接入

运行后用按钮或 `sceneEditor.SetSelectMode(SelectMode.Vertex)` 选择模式。鼠标悬停拾取区域，两次点击选取不同对象后调用几何辅助方法完成顶点或面吸附。

## 示例与限制

本目录没有独立演示场景。这些类型并未放在 Editor 目录或编辑器条件下，名称含 Editor 不表示仅编辑器可用。面片缓存不会自动跟随网格变化刷新；GL 绘制与吸附属于示例行为，不包含完整的撤销系统或建模编辑流程。
