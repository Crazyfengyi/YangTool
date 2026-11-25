# GizmosManager 调试绘制

## 功能与入口

[GizmosManager.cs](GizmosManager.cs) 暂存并绘制射线、球体射线、圆、圆环、扇形和盒体，按持续时间清理调试记录。

## 依赖与配置

场景中需要启用的 `GizmosManager`，并打开 Scene 视图的 Gizmos 显示。使用 Unity Gizmos，不依赖额外渲染组件。

## 最小接入

```csharp
GizmosManager.Instance.GizmosDrawRay(transform.position, transform.forward, 5f);
GizmosManager.Instance.GizmosDrawCircle(transform.position, 2f, 3f);
```

## 示例与限制

本目录没有演示场景。各绘制入口的 `time` 默认 5 秒；图形只用于调试可视化，不创建碰撞体，也不代表正式游戏画面的渲染效果。
