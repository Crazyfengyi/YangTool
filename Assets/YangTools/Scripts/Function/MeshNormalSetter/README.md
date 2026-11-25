# MeshNormalSetter 径向法线混合

## 功能与入口

[MeshNormalSetter.cs](MeshNormalSetter.cs) 将原法线与中心指向顶点的径向法线混合，并为当前 MeshFilter 创建网格副本。

## 依赖与配置

同物体需要 `MeshFilter`。`sourceMesh` 未设置时读取 `sharedMesh`；`center` 未设置时使用物体原点；`combineRate` 为零保留原法线，为一完全使用径向方向。源网格需要可读且具有法线数据。

## 最小接入

在组件上下文菜单执行 `Set Normals`，或调用：

```csharp
GetComponent<MeshNormalSetter>().SetNormals();
```

## 示例与限制

本目录没有演示场景。实现会实例化源网格并替换当前 MeshFilter 引用，不写回原资源；编辑器中记录 Undo。反复调用会产生新的网格实例，生命周期由使用方管理。这是径向混合法线，不是按相邻三角面重新计算平滑法线。
