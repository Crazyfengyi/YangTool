# RuntimeLogging

`RuntimeLogLevelController` 是场景级日志筛选控制器，可在运行时切换 Unity 全局 `Debug.unityLogger` 的日志级别。Inspector 提供 `All`、`Warning`、`Error`、`None` 选项。

将组件放入启动场景并在 Inspector 选择级别；它会跨场景保留，避免重复放置多个实例。该设置影响全局日志输出，不只是本模块；切换到 `None` 会抑制日志，调试完成后确认恢复需要的级别。
