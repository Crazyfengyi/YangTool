# TextColor 逐字符循环配色

## 功能与入口

[TextColor.cs](TextColor.cs) 把输入字符串中的每个字符包装为 TMP `<color>` 标签，并循环使用配置的颜色列表。

## 依赖与配置

同物体需要 `TMP_Text`，Inspector 的 `colors` 至少配置一种颜色。Awake 获取组件、开启富文本并处理当前文字。

## 最小接入

```csharp
GetComponent<TextColor>().SetText("彩色文字");
```

## 示例与限制

本目录没有演示场景。空颜色列表无法完成配色；遍历按 C# char 处理，不保证完整处理复合字符。输入含已有 TMP 标签时也会逐字符包装，应先评估兼容性。调用会直接替换文本内容。
