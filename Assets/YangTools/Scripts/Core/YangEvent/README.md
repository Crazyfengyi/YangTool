# YangEvent 事件系统

主线程同步事件系统 支持对象绑定 全局监听 优先级和分组释放
只依赖 Unity 和标准 C# 核心文件为 `YangToolEventManager.cs`

## 快速使用

事件可以继承 `EventMessageBase` 也可以是普通 C# 类

```csharp
public class EnemyDefeated : EventMessageBase
{
    public string EnemyId;
}
```

在组件启用时注册 禁用时注销 保存返回的句柄可精确释放自己的监听

```csharp
private EventInfo listener;

private void OnEnable()
{
    listener = this.AddEventListener<EnemyDefeated>(OnEnemyDefeated);
}

private void OnDisable()
{
    YangExtend.RemoveEventListener(listener);
    listener = null;
}

private void OnEnemyDefeated(EventData data)
{
    EnemyDefeated message = (EnemyDefeated)data.Args;
    Debug.Log(message.EnemyId);
}
```

发送继承事件或普通对象事件

```csharp
new EnemyDefeated { EnemyId = "slime" }.SendEvent();
YangExtend.SendEvent<EnemyDefeated>(new EnemyDefeated { EnemyId = "slime" });

// 普通 C# 类无需继承 EventMessageBase
YangExtend.SendEvent<MyMessage>(new MyMessage());
YangExtend.SendEvent(typeof(MyMessage), new MyMessage());
```

以上 `MyMessage` 由业务自行定义 不会隐式转换或检查消息参数的类型

## 分组与全局监听

```csharp
private readonly YangEventGroup events = new YangEventGroup();

private void OnEnable()
{
    events.AddListener<EnemyDefeated>(OnEnemyDefeated);
}

private void OnDisable()
{
    events.Dispose();
}
```

`RemoveAllListener()` 和 `Dispose()` 只注销当前组的监听 不影响其他组或同名事件
重复释放安全 释放后仍可重新注册
同一分组中的相同事件类型和相等回调只注册一次
分组监听不依赖 `YangToolsManager.DontDestoryObject` 普通 C# 服务也可以使用

单条全局监听可显式使用空持有者

```csharp
EventInfo listener = YangExtend.AddEventListener<EnemyDefeated>(null, OnEnemyDefeated);
YangExtend.RemoveEventListener(listener);
```

全局监听必须主动注销 否则回调和闭包会继续被持有
对象绑定的监听在对应事件派发时自动清理已销毁目标 即使监听被禁用也会清理
没有再次派发的事件不会主动巡检 因此仍应在 `OnDisable` 或 `OnDestroy` 中释放
按对象注销会删除该对象全部监听 包括同一对象的多个优先级和不同事件
真正的 `null` 不会批量删除全局监听 已销毁 Unity 对象也不会变成全局监听

## 路由与执行顺序

- 默认按 `typeof(T).FullName` 精确匹配 监听基类不会收到派生类型事件
- 可通过扩展方法的 `eventName` 参数指定自定义名称 注册与发送必须使用相同名称
- `sortId` 越小越先执行 同优先级按注册顺序执行
- 同一个 `EventInfo` 重复添加不会重复触发 不同句柄即使回调相同仍为独立监听
- `listener.isEnabled = false` 暂停该监听 重新启用不会改变注册顺序
- 一条监听抛出异常时记录事件名称和异常 后续监听继续执行

## 派发中修改监听

- 注销立即生效 本轮尚未执行的监听不再执行
- 新增或注销后重新注册的监听不加入已有派发 从下一次发送开始生效
- 嵌套发送属于新一轮派发 能看到嵌套发送前的最新注册状态
- `RemoveForKey` 注销指定名称的全部监听 `Clear` 注销管理器全部监听 二者会立即使旧快照失效
- 清理后可以重新注册 同一分组再次添加同一回调也能恢复被外部清理的注册

监听变化后才重建有序快照 每次发送保留独立 `EventData` 允许回调保存参数
热更新操作结束时应释放自己的句柄 不要调用全局 `Clear()`

## Unity 生命周期与限制

注册 注销和发送都必须在 Unity 主线程完成 单例创建锁不代表集合线程安全
后台线程消息应先由业务切回主线程 本系统不提供后台队列
每次进入 Play Mode 都通过 `SubsystemRegistration` 清理单例的监听 兼容关闭 Domain Reload
关闭 Scene Reload 时业务仍需在适当的运行模式初始化入口重新注册监听
空事件名 空回调和空事件类型会抛出参数异常 空消息参数可以正常发送

## 回归测试

在 Unity Test Runner 的 EditMode 中运行 `YangTools.Event.Tests`
测试覆盖注销隔离 全局监听 优先级 重复注册 派发中变更 嵌套发送 异常与对象销毁
任务桥测试仅在启用 `YANGTOOLS_QUEST_INTEGRATION` 时执行
