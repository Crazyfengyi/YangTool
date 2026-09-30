# NetIcon

`NetIcon` 要求同一物体上存在 UGUI `Image`，通过 `LoadImageByWeb(string url)` 下载网络图片并创建 Sprite 显示到图像。

```csharp
GetComponent<NetIcon>().LoadImageByWeb(imageUrl);
```

新的请求会停止旧请求；下载失败会记录警告；组件销毁时会释放自身创建的纹理/Sprite。调用方需提供可访问的图片 URL，并考虑网络失败/超时。移动平台需确认网络权限、HTTPS/TLS 和证书策略；不要将不可信 URL 用于敏感流程。
