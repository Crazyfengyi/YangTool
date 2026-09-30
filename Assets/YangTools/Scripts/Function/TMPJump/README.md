# TMPJump

`TMPJump` 使用 DOTween 为 TMP 文本字符创建错位跳跃及淡入淡出效果，可配置循环、颜色、位移、每字延迟、时长和缓动曲线。

```csharp
GetComponent<TMPJump>().DoJump();
```

将脚本与 `TMP_Text` 放在同一物体，按 Inspector 设置动画参数；项目需包含 DOTween 与 Odin Inspector 依赖。文本变化后重新播放前应确认旧动画被组件正确终止，避免同时驱动同一文本网格。
