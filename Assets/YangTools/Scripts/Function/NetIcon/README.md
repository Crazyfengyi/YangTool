# NetIcon 网络图片

## 功能与入口

[NetIcon.cs](NetIcon.cs) 下载 URL 指向的图片，为 UGUI Image 创建并显示 Sprite。

## 依赖与配置

同物体需要 `Image`，联网使用 UnityWebRequest。调用方传入可访问的图片地址，并确保目标平台网络权限和证书配置可用。

## 最小接入

```csharp
GetComponent<NetIcon>().LoadImageByWeb(imageUrl);
```

## 示例与限制

本目录没有演示场景。新请求会停止旧请求，失败记录警告；组件销毁时释放自己创建的纹理与 Sprite。未提供跨组件共享缓存或业务层重试策略。
