# Parabola

[ParabolaPath.cs](ParabolaPath.cs) 提供抛物线轨迹的位置、速度和离散点计算，`Missile` 组件用于将轨迹应用于场景对象。

```csharp
var path = new ParabolaPath(start, end, height, gravity);
Vector3 position = path.GetPosition(t);
Vector3 velocity = path.GetVelocity(t);
```

`height` 表示相对起终点较高者再抬升的高度，`gravity` 应为负值。`GetParabolaList` 当前实现的数组长度与循环边界不一致，调用时可能发生越界；使用前请先在项目中修复/验证该方法。轨迹是运动学计算，不会自动处理碰撞、寻路或目标移动。
