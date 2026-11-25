# RuntimeLogging 全局日志筛选

## 功能与入口

[RuntimeLogLevelController.cs](RuntimeLogLevelController.cs) 在 Awake 设置 `Debug.unityLogger`，支持全部、警告及以上、仅错误和关闭四档。

## 依赖与配置

使用 `XYBTMiniGame` 命名空间，仅依赖 Unity。将组件放在启动场景的独立根物体上，在 Inspector 选择日志级别；首个实例跨场景保留，重复实例会移除自身组件。

## 最小接入

在进入运行前配置所需级别并启动场景。当前没有公开切换方法或 OnValidate 应用逻辑；运行中只修改 Inspector 字段不会重新调用 Awake。

## 示例与限制

本目录没有独立演示场景。筛选影响 Unity 全局日志，不仅限于此模块；`None` 关闭日志，销毁组件不会自动恢复之前的日志设置。
