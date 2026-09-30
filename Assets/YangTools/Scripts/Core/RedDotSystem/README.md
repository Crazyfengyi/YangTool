# 红点树系统

本目录提供按路径组织的红点节点树。叶子节点保存业务数值，父节点自动汇总子节点数值，并在值变化时通知监听者，适用于未读数、待处理数量等 UI 提示。

## 主要文件

- `RedDotMgr.cs`：全局入口 `RedDotMgr.Instance`，负责路径解析、节点创建、监听管理和父链刷新。
- `RedDotTreeNode.cs`：节点值、父子关系及值变化回调。
- `RedDotPath.cs`、`RangeString.cs`：路径与节点键辅助结构。
- `RedDotExample.cs`：场景组件用法示例。

## 使用

```csharp
Action<RedDotTreeNode> onChanged = node => Debug.Log(node.Value);
RedDotTreeNode node = RedDotMgr.Instance.AddListener("Mail/Unread", onChanged);
node.ChangeValue(1); // 叶子节点的值变化会向父级汇总
RedDotMgr.Instance.RemoveListener("Mail/Unread", onChanged);
```

路径使用 `/` 分段，不能以分隔符开头或结尾、不能有连续分隔符，也不能留下 `{0}` 占位符。父节点的值由子节点汇总，不应直接设置；组件销毁或不再关心该节点时应移除监听。
