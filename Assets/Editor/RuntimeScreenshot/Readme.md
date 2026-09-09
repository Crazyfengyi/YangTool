# RuntimeScreenshotEditor

Unity 编辑器运行模式截图工具。脚本仅在 `UNITY_EDITOR` 下编译，不会被打入正式构建包，也不需要在场景中挂载组件。

## 快速使用

1. 确认 `RuntimeScreenshotEditor.cs` 位于 `Assets/Editor/RuntimeScreenshot/` 目录下。
2. 等待 Unity 编译完成后，进入 **Play Mode**。
3. 在 Game 视图或 Unity 编辑器中按 `M` 键。
4. 截图会保存到桌面的 `UnityScreenshots` 文件夹。

默认文件名格式为：

```text
Screenshot_yyyyMMdd_HHmmss_fff.png
```

如果同名文件已经存在，工具会自动追加 `_1`、`_2` 等后缀，不会覆盖原截图。

## 快捷键

默认快捷键为 `M`，对应 Unity Shortcut Manager 中的命令：

```text
Runtime Screenshot/Capture
```

如与项目或其他插件的快捷键冲突，可在 Unity 菜单 **Edit → Shortcuts...** 中搜索 `Runtime Screenshot/Capture` 并重新绑定。截图正在等待 Unity 延迟调用时，重复按键会被忽略。

## 截图条件

只有满足以下条件时才会执行截图：

- Unity 当前处于 Play Mode。
- 编辑器未处于暂停状态。
- 上一次截图请求已经完成调度。

退出 Play Mode、暂停编辑器或无法获取桌面路径时，工具会放弃本次截图并在 Console 输出原因。截图分辨率取触发时的 `Screen.width` 和 `Screen.height`；实际文件由 Unity 的 `ScreenCapture.CaptureScreenshot` 在后续帧写入。

## 输出与日志

保存目录由当前操作系统的桌面路径决定：

```text
桌面/UnityScreenshots/
```

成功或失败信息会输出到 Unity Console，其中成功日志包含完整文件路径和截图分辨率。若文件暂未出现在目录中，请等待一帧或检查 Unity Console 中的错误信息。

## 注意事项

- 这是编辑器辅助工具，不能在打包后的 Player 中使用。
- 目标项目必须启用 Unity Editor 脚本编译；不要将脚本移动到运行时程序集目录。
- 截图文件保存在桌面，不会自动导入 Unity 项目，也不会自动清理历史文件。
- 如果截图内容异常，请先确认 Game 视图分辨率、窗口焦点以及当前是否真的处于 Play Mode。

