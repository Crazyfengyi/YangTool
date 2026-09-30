# 行为树

本目录实现轻量行为树节点：Root 节点组织子节点，Sequence 顺序执行，Selector 逐个尝试，Parallel 按成功/失败条件结束；Action 节点承载具体业务动作。

## 目录职责

- `TaskTreeBase/`：树实例、节点基类、根节点及 Sequence/Selector/Parallel 等组合节点。
- `ActionNode/`：叶子动作节点，包括延时、日志等示例。
- `DecoratorNode/`：包装/转发单个子节点的节点类型。
- `TestBT.cs`：通过代码构造行为树并重启的 MonoBehaviour 示例。

## 使用

```csharp
var root = new RootNode();
var tree = new TaskTree(root);
var sequence = new SequenceNode();
root.AddChild(sequence);
sequence.AddChild(new DelayNode(1f));
tree.StartRun();
```

节点树由调用方组装；增加组合节点时正确设置父子关系，并为业务动作派生 `ActionNodeBase`。延时节点通过 `YangTimerManager` 推进；`TestBT` 中的示例还展示了结束事件订阅和重启。
