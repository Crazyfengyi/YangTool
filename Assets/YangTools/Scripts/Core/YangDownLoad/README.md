# HTTP 字节下载

`YangDownLoad` 基于 `UnityWebRequest.Get` 提供协程式 HTTP GET，将响应数据作为 `byte[]` 返回，并通过回调报告网络错误。

## 主要文件

- `YangDownLoad.cs`：惰性创建并挂到 `YangToolsManager.DontDestoryObject` 的下载组件，以及静态 `DownLoad` 协程。

## 使用

```csharp
StartCoroutine(YangDownLoad.DownLoad(
    url,
    bytes => Debug.Log($"收到 {bytes.Length} 字节"),
    error => Debug.LogError(error)));
```

下载由 Unity 主线程上的协程推进；组件首次创建依赖 YangTools 全局管理物体已初始化。该接口只返回原始字节，不负责文件落盘、断点续传、重试策略或下载进度，相关需求由调用方处理。
