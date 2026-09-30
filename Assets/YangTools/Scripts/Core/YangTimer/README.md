# 计时器

`YangTimerManager` 提供帧计时、秒计时和循环秒计时。每个 `YangTimer` 返回一个可取消句柄，可绑定 Unity 对象以在对象销毁后停止回调，也可选择使用缩放或非缩放时间。

## 主要文件

- `YangTimerManager.cs`：注册、按帧更新和移除计时器。
- `YangTimer.cs`：句柄、基础计时信息及帧/秒计时实现。
- `TimeTool.cs`：NTP 网络校时、当前时间和日期/时间戳转换工具；由 `UnityLoopScript` 初始化并更新。

## 使用

```csharp
YangTimer timer = YangTimerManager.AddSecondTimer(
    2f, OnTimeout, isScaled: false, holder: this);
// 提前取消
YangTimerManager.RemoveTimer(timer);
```

计时器管理器由 `YangToolsManager` 更新。`AddFrameTimer` 的延迟以帧为单位；`AddSecondTimer` 的 `loopCount` 为 0 时不执行、-1 表示无限次。回调中创建的新计时器从下一轮管理器更新开始处理；绑定对象销毁或回调抛异常时计时器会终止。`TimeTool.GetTime()` 在成功校时后返回递增的校准时间，未获得校时值时回退到本地时间；网络校时依赖设备可访问 NTP 服务。
