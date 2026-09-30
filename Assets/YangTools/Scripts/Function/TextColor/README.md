# TextColor

`TextColor` 为 TMP 文本逐字符套用循环颜色列表。它要求同物体存在 `TMP_Text`，并在 Inspector 的 `colors` 列表配置至少一种颜色。

```csharp
GetComponent<TextColor>().SetText("彩色文字");
```

组件启用时会开启 TMP 富文本，并按字符顺序生成 `<color>` 标签；`colors` 为空时当前实现无法生成有效颜色序列，因此务必至少配置一项。调用 `SetText` 会替换原始文本格式，若要保留 TMP 标签/富文本，请先评估逐字符包装与标签的兼容性。
