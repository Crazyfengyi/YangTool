# CreateInMesh

`CreateInMeshPoint` 要求同一物体有 `MeshFilter`，在 `Start` 缓存其共享网格顶点、法线和三角形，并按三角形面积加权随机生成 Inspector 指定的 `spawnObject`。按空格键触发生成，落点朝向插值法线；生成对象以该网格物体为父级。它当前是示例型组件，没有公开的生成方法。

同文件的 `WeightedRandom(float[] weights)` 可独立用于按权重抽取索引：

```csharp
var picker = new WeightedRandom(new[] { 1f, 3f, 2f });
int index = picker.GetRandomIndex();
```

网格必须包含可读的顶点、三角形和法线数据，权重数组应非空且权重总和大于零。运行时修改网格后组件不会自动重建缓存。
