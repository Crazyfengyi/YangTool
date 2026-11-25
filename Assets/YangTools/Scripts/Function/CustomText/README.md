# CustomText 注音与打字显示

## 功能与入口

[CustomText.cs](CustomText.cs) 继承 `TextMeshProUGUI`，支持注音标签及逐字淡入。`CustomTextPreprocessor` 解析注音范围和逐字间隔，常规富文本继续交给 TMP 处理。

## 依赖与配置

依赖 TMP 和 UGUI。注音模板通过 Resources 路径 `Ruby/RubyText` 加载，模板需带 `TextMeshProUGUI`；中英文所需字符应由主文本和注音的字体支持。

## 最小接入

```csharp
customText.ShowTextByTyping("<r=nǐ hǎo>你好</r>！", () => Debug.Log("播放完毕"));
```

`<r=注音>文字</r>` 为注音标签，独立非负数字标签如 `<0.15>` 设置前一可见字符后的等待秒数。打字动画使用非缩放时间。

## 示例与限制

本目录没有独立演示场景。注音模板缺失时保留主文本并记录警告；已有 TMP 文本直接换组件前需确认原有引用。复杂排版效果仍受 TMP 网格、字体和布局设置影响。
