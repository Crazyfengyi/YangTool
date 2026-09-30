# MageTower

[MageEngineer.cs](MageEngineer.cs) 是一个法师塔/弹幕组合的演示性模拟：组合子弹与 Buff 法术，预测发射结果并驱动射击。包含 `MageEngineer`、`ShooterSystem`、`Bullet` 及法术枚举/行为等类型。

```csharp
var engineer = new MageEngineer();
engineer.Shoot();
```

这是独立示例逻辑，不是通用战斗服务；法术列表和模拟更新需要调用方管理。用于场景表现时还需自行创建/配置弹体对象及其运动、命中和销毁流程。
