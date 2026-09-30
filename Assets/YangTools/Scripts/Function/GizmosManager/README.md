# GizmosManager

`GizmosManager` 是场景中的调试绘制组件，可在 Scene 视图中短时显示射线、球体射线、圆、圆环、扇形与盒体。绘制信息由组件内部保存并按持续时间清理。

```csharp
GizmosManager.Instance.GizmosDrawRay(transform.position, transform.forward, 5f);
GizmosManager.Instance.GizmosDrawCircle(transform.position, 2f, 3f);
```

场景中需存在启用的 `GizmosManager` 组件；绘制是编辑/调试可视化，不会创建碰撞体，也不等于游戏内渲染。Scene 视图 Gizmos 开关关闭时不可见。各方法的 `time` 默认为 5 秒。
