# 通用对象池

本目录提供按类型和 PoolKey 管理的异步对象池、同步入口、自动回收包裹，以及独立的引用池。池物品通过 `IPoolItem<T>` 生命周期方法创建、启用、回收和销毁。

## 主要接口

- `YangObjectPool.Get<T>` / `GetSync<T>`：借出对象，返回 `(是否新建, 对象)`。
- `GetAutoPackage<T>` / `GetAutoPackageSync<T>`：取得 `IDisposable` 包裹，在 `using` 结束时回收。
- `Recycle(item)`：显式回收对象；`CreatePool<T>(key)` 可提前创建具名池。
- `DefaultObjectPoolItem`：使用 ResourceManager 实例化 GameObject，并默认回收到 `YangToolsManager.GamePoolObject`。
- `ReferencePool.Get<T>()` / `Recycle<T>(value)`：缓存普通引用对象，不执行 `IPoolItem` 生命周期。

## 使用

```csharp
var (isNew, item) = await YangObjectPool.Get<DefaultObjectPoolItem>(
    "Effects", "Effects/Hit", parent);
YangObjectPool.Recycle(item);
```

自定义物品需实现 `IPoolItem<T>`，构造参数只在新建时用于构造和 `OnCreate`。同步接口会阻塞当前线程，资源尚未完成异步加载时不要在主线程调用。对象必须归还到所属池，且不可重复回收。
