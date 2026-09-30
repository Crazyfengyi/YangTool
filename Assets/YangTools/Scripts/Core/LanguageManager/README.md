# 多语言管理

`LanguageManager` 根据当前语言类型，从 Luban 生成的 `cfg.language` 配表中读取文本。当前实现通过反射按语言类型属性名取值，适合将界面文案集中维护在语言表中。

## 主要文件

- `LanguageManager.cs`：单例入口，持有当前 `languageType`，并在 `Start` 时从 `GameTableManager.Instance.Tables.TBLanguage` 初始化语言表和属性映射。

## 使用

确保先加载完成游戏配置表，再查询语言文本：

```csharp
string text = LanguageManager.Instance.GetLanguage("textStrId_1");
```

`languageType.ToString()` 必须与生成的 `Language` 类型属性名一致，key 必须存在于语言表。当前缺失语言项时返回带 key 的提示字符串；不要在配置尚未加载、`LanguageManager.Start` 尚未完成时调用。
