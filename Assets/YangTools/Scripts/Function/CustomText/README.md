# CustomText

`CustomText` 继承 TextMeshProUGUI，提供注音标签和逐字淡入显示；文本预处理器 `CustomTextPreprocessor` 识别注音范围与逐字间隔。注音 Prefab 通过 Resources 地址 `Ruby/RubyText` 加载，Prefab 需含 `TextMeshProUGUI`。

```csharp
customText.ShowTextByTyping("<r=nǐ hǎo>你好</r>！", () => Debug.Log("播放完毕"));
```

可用 `<r=注音>文字</r>` 标注注音；独立的非负数字标签（如 `<0.15>`）设置前一可见字符后的打字间隔。普通 TMP 富文本标签会保留并交由 TextMeshPro 解析。注音 Prefab 缺失时只显示主文本并记录警告；请在 Resources 下保持上述资源路径。打字动画使用非缩放时间。
