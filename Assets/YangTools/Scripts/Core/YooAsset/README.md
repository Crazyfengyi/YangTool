# YooAsset 扩展

本目录补充 YooAsset 项目内使用的资源引用组件、批量加载操作和句柄等待扩展；YooAsset 包初始化、构建及运行模式由启动流程和 YooAsset 配置负责。

## 主要文件

- `GameObjectAssetReference.cs`：Inspector 中选择 GameObject 后保存其 Asset GUID；运行时从 `DefaultPackage` 加载并实例化到组件节点，销毁时释放句柄。
- `LoadAssetsByTagOperation.cs`：按标签并发加载资源，完成后公开 `AssetObjects`，并由调用方通过 `ReleaseHandle()` 释放句柄。
- `AssetOperationHandleExtension.cs`：为 `AssetHandle` 提供同步等待完成的扩展方法。

## 使用

```csharp
IEnumerator LoadByTag()
{
    var operation = new LoadAssetsByTagOperation<GameObject>("Enemies");
    YooAssets.StartOperation(operation);
    yield return operation;
    if (operation.Status == EOperationStatus.Succeed)
    {
        foreach (GameObject asset in operation.AssetObjects)
        {
            Debug.Log(asset.name);
        }
    }
    operation.ReleaseHandle();
}
```

`GameObjectAssetReference` 固定从 `DefaultPackage` 按 GUID 解析资源，使用前需初始化并设置默认包。批量加载完成后应按生命周期释放已创建的句柄；`WaitForAsyncOperationComplete()` 会阻塞等待，不要在需要异步响应的主线程流程中滥用。
