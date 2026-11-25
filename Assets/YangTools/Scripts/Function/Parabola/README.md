# Parabola 抛物线轨迹

## 功能与入口

[ParabolaPath.cs](ParabolaPath.cs) 根据起终点、抬升高度和重力计算位置及运动时长。[Missile.cs](Missile.cs) 是沿预计算路径移动物体的演示。

## 依赖与配置

轨迹工具仅依赖 Unity 数学类型。`height` 表示相对起终点较高者再抬升的高度，`gravity` 应为负数。Missile 需要目标 Transform 和重力参数。

## 最小接入

```csharp
var path = new YangTools.ParabolaPath(start, end, height, gravity);
Vector3 position = path.GetPosition(t);
```

设置 `IsClampStartEnd` 后，通过 `Time` 属性推进可将位置限制在起点和终点之间。

## 示例与限制

本目录没有独立演示场景。Missile 只在 Start 采样目标位置，实际抬升高度使用 10 到 16 的随机值，未读取公开 `height` 字段；到终点后仅停用自身组件，不是追踪导弹，也不处理碰撞。

当前 `GetParabolaList` 分配的数组长度与循环上界不一致，会发生越界；`GetVelocity` 在非零时刻重复叠加了初始 Y 速度。本次仅记录现状，调用这两个入口前需单独修复和验证。
