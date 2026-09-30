# SelectPoint

该目录用于场景网格选择与吸附辅助。`SceneEditor` 组件提供顶点、三角形/平面等选择模式及绘制状态入口；`EditorHelper` 提供包围盒、射线拾取三角形、面片范围和对象吸附计算。

把 `SceneEditor` 放在用于交互的场景对象上，在 Inspector 配置相机、选择控件及材质引用，再按需要调用 `SetSelectMode(...)`。拾取流程使用屏幕射线和 `MeshCollider`；目标网格需有可用 Collider/三角形数据，摄像机与 UI 输入需要正确配置。

`EditorHelper` 是几何辅助 API；尽管目录名含 Editor，不要假定其中所有代码只在 Unity Editor 可用。运行时/编辑器程序集使用前应核对当前 asmdef 和平台编译设置。
