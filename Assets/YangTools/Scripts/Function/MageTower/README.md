# MageTower 法术序列模拟

## 功能与入口

[MageEngineer.cs](MageEngineer.cs) 在 `YangToolMageEngineer` 命名空间内提供硬编码法术序列演示。`ShooterSystem` 顺序消费常驻增益、一次性增益和逻辑子弹，支持包装器与多重施法的组合计算。

## 依赖与配置

这是普通 C# 类的模拟，不是场景 MonoBehaviour。无需 Inspector 绑定，调用方持有 `MageEngineer` 并决定触发时间。

## 最小接入

```csharp
var engineer = new YangToolMageEngineer.MageEngineer();
engineer.Shoot();
```

可用 `ShooterSystem.Forecast()` 观察下一组候选法术，`Reset()` 恢复一次性法术的使用状态。

## 示例与限制

示例序列在构造方法中创建，没有独立场景。`Bullet.OnStart`、`OnUpdate`、`OnEnd` 为无表现的扩展钩子；当前不会实例化 Unity 弹体、处理命中或自动推进更新。它与 Battle、NoitaStyleSpells 均为不同实现。
