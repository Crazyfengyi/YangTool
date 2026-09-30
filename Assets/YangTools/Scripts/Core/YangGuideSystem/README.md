# 新手引导

本目录提供引导步骤数据、目标 UI 收集和遮罩/指示控制。当前 `YangGuideManager` 中包含项目流程专用的引导方法，集成时需与项目的引导窗口、存档及 UI 资源配合。

## 主要文件

- `YangGuideManager.cs`：引导信息注册和项目引导步骤控制入口。
- `GuideInfoCollect.cs`：在目标 UI 上配置 `guideID`，启动时收集对应 `RectTransform`；Button 会被绑定为下一步触发器。
- `GuideCircleMaskController.cs`、`GuideRectMaskController.cs`：圆形与矩形遮罩显示控制。
- `Other/`、`Res/`：目录内配套内容资源。

## 接入

在需要引导的目标组件上配置非零 `guideID`，并确保场景中存在并初始化引导窗口及遮罩资源。目标信息由 `GuideInfoCollect` 自动注册；手动注册时可按源码构造数据：

```csharp
var info = new GuideInfo(guideId, targetRectTransform);
YangGuideManager.Instance.AddGuideInfoDict(guideId, info, targetRectTransform);
```

引导步骤和窗口名称目前与项目 UI/业务逻辑耦合；接入其他项目时，应先替换 `YangGuideManager` 中的具体流程与资源引用，不要把示例中的业务步骤当作通用引导配置系统。
