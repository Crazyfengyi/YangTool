# CreateInMesh 网格表面采样

## 功能与入口

[CreateInMeshPoint.cs](CreateInMeshPoint.cs) 在 `Start` 缓存共享网格数据，按三角形面积加权选择面片，再以重心坐标在面内均匀采样并生成对象。朝向使用面片三个顶点法线的平均值。

## 依赖与配置

同物体需要 `MeshFilter`，网格须可读并具有有效顶点、三角形与法线。Inspector 配置 `spawnObject`，生成物体以网格物体为父级。

## 最小接入

运行后按空格键生成一个对象。组件的 `Spawn` 为内部入口，没有公开生成方法。同文件的权重工具可独立使用：

```csharp
var picker = new WeightedRandom(new[] { 1f, 3f, 2f });
int index = picker.GetRandomIndex();
```

## 示例与限制

本目录没有演示场景。权重数组应非空且总和大于零；运行中修改网格不会自动重建采样缓存。该组件不负责碰撞避让、生成上限或对象回收。
