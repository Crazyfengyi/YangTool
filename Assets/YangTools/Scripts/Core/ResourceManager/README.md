# 资源管理

`ResourceManager` 为项目封装统一的资源加载入口，底层接入 YooAsset，并提供异步加载、预加载、实例化及引用计数式资源获取接口。调用前需按项目启动流程初始化 YooAsset 和默认资源包。

## 主要文件

- `ResourceManager.cs`：集中处理 Asset、子资源、场景、Sprite、音频及 GameObject 的加载和缓存；提供 `AcquireAssetAsync` / `ReleaseAsset` 与普通加载接口。

## 使用

```csharp
using YangTools.Scripts.Core.ResourceManager;

GameObject prefab = await ResourceManager.LoadAssetAsync<GameObject>("UI/Example");
GameObject instance = await ResourceManager.InstantiateGameObject("UI/Example", parent, false);
```

`LoadAssetAsync` 适合使用管理器的普通缓存；需要生命周期引用计数时配对使用 `AcquireAssetAsync` 和 `ReleaseAsset`。`ClearCache` 会清理缓存句柄，调用前应确保业务不再使用相应资源。资源地址必须与当前 YooAsset 包的地址规则一致。
