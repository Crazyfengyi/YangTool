# MeshNormalSetter

[MeshNormalSetter](MeshNormalSetter.cs) 是用于网格法线处理的组件，公开 `SetNormals()` 入口，可在需要时重算目标网格法线。

```csharp
GetComponent<MeshNormalSetter>().SetNormals();
```

按组件 Inspector 配置目标 MeshFilter/网格引用后再调用。运行时修改网格前确认 Mesh 可读且为该对象独占；共享 Mesh 可能影响其他实例，必要时先实例化网格副本。具体目标字段以当前 Inspector 实现为准。
