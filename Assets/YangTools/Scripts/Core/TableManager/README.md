# 配置表与表资源管理

`GameTableManager` 负责加载 YooAsset 中标记为 `LubanData` 的配置文本，并用 Luban 生成的 `cfg.Tables` 构建运行时表对象；同时按地址异步加载并缓存其他动态资源。

## 主要文件

- `GameTableManager.cs`：配置初始化、JSON 文件解析、动态资源预加载和按地址读取。
- `Tables`、`cfg`、`Bright.Serialization`：由项目表格流水线生成或提供的配置类型与序列化支持，不在本目录维护。

## 使用

```csharp
await GameTableManager.Instance.LoadAllConfigs();
var tables = GameTableManager.Instance.Tables;
```

初始化前应先准备好 YooAsset 的 `DefaultPackage`，并确保配置资源带有 `LubanData` 标签。动态资源可通过 `PreloadAssetsAsync(addresses)` 预加载，之后用 `GetPreloadedAsset<T>(address)` 查询；异步 `LoadAssetAsync<T>(address)` 会按地址缓存并合并并发加载。加载失败时应检查日志和资源地址。
