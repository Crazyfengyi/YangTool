# QuestSystem 任务系统

QuestSystem 是一个基于 `ScriptableObject` 配置的任务系统，负责任务状态、目标条件、进度、每日刷新、存档和奖励事件分发。

## 目录结构

```text
Assets/YangTools/QuestSystem/
├─ Script/                         运行时代码
│  ├─ QuestData.cs                 任务配置、目标和条件数据
│  ├─ QuestEvents.cs               进度事件和任务事件
│  ├─ QuestRuntime.cs              任务运行时逻辑
│  ├─ QuestManager.cs              通用任务管理器单例
│  ├─ QuestManagerBootstrap.cs     项目默认启动引导
│  ├─ QuestServices.cs             服务接口和内存实现
│  └─ QuestProjectAdapters.cs      Yang、BagMgr、YooAsset 适配器
├─ Editor/QuestSystem/             QuestData 中文 Inspector
└─ Data/                           任务配置资源和管理器预制体
```

## 快速开始

### 1 创建管理器

推荐直接使用 `Assets/YangTools/QuestSystem/Data/QusetManager.prefab`。手动创建时，在同一个 GameObject 上添加：

`Add Component → YangTools → Quest → Quest Manager`

`Add Component → YangTools → Quest → Quest Manager Bootstrap`

`QuestManager` 是场景单例：

- 第一个实例会成为 `QuestManager.Instance`
- 重复实例会自动销毁
- 默认跨场景保留
- `Update` 会自动驱动时间条件和在线时长，不要再额外每帧调用 `Tick`

`QuestManagerBootstrap` 默认会自动完成项目接入：

- 注入 Yang 存档和 `BagMgr` 道具服务
- 桥接全局 `QuestProgressEvent` 和任务状态事件
- 等待 YooAsset 就绪后加载 `DefaultPackage` 中标签为 `SO` 的全部任务配置

Bootstrap Inspector 可配置自动加载开关、资源包名称、任务资源标签和等待时长，默认值分别为开启、`DefaultPackage`、`SO` 和 30 秒。超时或加载异常时会记录错误并完成当前注册批次，管理器仍可运行，之后可以手动调用 `RegisterQuest` 和 `CompleteRegistration`。

如果需要纯通用模式或自行注入服务，可以移除或禁用 `QuestManagerBootstrap`，再通过代码调用 `ConfigureServices` 和任务注册接口。


### 2 创建任务配置

在 Project 窗口选择：

`Create → Game → Quest → QuestData`

任务配置字段说明：

| 字段 | 说明 |
| --- | --- |
| 任务稳定 ID | 必填且全局唯一，用于前置任务和存档索引 |
| 任务类型 | 普通、每日、现金或收集 |
| 前置任务 ID 列表 | 只有列表中的任务完成后才可接取 |
| 任务目标列表 | 按顺序完成的目标 |
| 任务奖励列表 | 完成任务后通过奖励事件发放 |

任务 ID 必须全局唯一且稳定。目标和条件不需要手动填写 ID，系统会按列表索引生成内部存档键，已上线任务不要随意调整目标或条件顺序。

### 3 注册任务

可以直接注册任务资源：

```csharp
QuestManager manager = QuestManager.Instance;
manager.Initialize(new[] { questData });
```

也可以批量注册：

```csharp
manager.RegisterQuests(questDataList);
```

注册完成后会触发 `DataLoaded` 事件。启用 Bootstrap 后无需额外编写 YooAsset 加载代码。需要完全手动控制时，可以使用：

```csharp
private YooAssetQuestDataLoader questLoader;

private async void Start()
{
    QuestManager manager = QuestManager.Instance;
    manager.ConfigureServices(new YangQuestSaveStore(), new BagQuestItemService());
    manager.Initialize();

    questLoader = new YooAssetQuestDataLoader();
    await questLoader.LoadAsync(manager, "DefaultPackage", "SO");
}

private void OnDestroy()
{
    questLoader?.Dispose();
}
```

`ConfigureServices` 必须在注册任务数据前调用。未注入服务时，系统使用内存存档和空背包服务，适合测试但不会写入正式存档。`QuestManager.IsServicesConfigured` 可用于判断是否已注入服务。

## 进度和状态

### 接取和完成

```csharp
manager.AcceptQuest("quest_001");
manager.CompleteQuest("quest_001");
manager.ClaimReward("quest_001");
```

任务状态流转为：
`锁定 → 可接取 → 进行中 → 已完成 → 已领奖`

目标勾选“条件满足后自动完成”时，条件满足会自动完成目标；否则需要调用 `CompleteQuest`。

### 提交进度事件

```csharp
manager.ReportProgress(new QuestProgressEvent(
    QuestProgressEventType.Kill,
    "enemy_001",
    amount: 1f));
```

条件类型：

- `EventCount`：每次匹配事件累加 `Amount`
- `EventOnce`：第一次匹配事件后直接满足
- `Time`：按真实 UTC 时间累计，目标数量单位为分钟
- `OnLineTime`：仅在应用前台累计，目标数量单位为分钟
- `ItemNum`：从 `IQuestItemService` 读取当前道具数量，`TargetId` 填道具 ID

`ItemNum` 条件会由管理器自动刷新，不需要手动伪造进度事件。

## 奖励处理

管理器只负责发送 `RewardIssued` 事件，不直接修改金币或背包，业务层负责监听并发放奖励：

```csharp
private void OnEnable()
{
    if (QuestManager.Instance != null)
    {
        QuestManager.Instance.RewardIssued += OnRewardIssued;
    }
}

private void OnDisable()
{
    if (QuestManager.Instance != null)
    {
        QuestManager.Instance.RewardIssued -= OnRewardIssued;
    }
}

private void OnRewardIssued(QuestRewardEvent eventData)
{
    QuestRewardData reward = eventData.RewardData;
    // 根据 RewardType 和 TargetKey 发放实际奖励
}
```
奖励字段说明：

- `RewardType.Item`：`TargetKey` 填道具 ID，`Count` 填数量
- 其他奖励：`TargetKey` 按业务约定填写目标键，`Count` 填数量
- 奖励列表不需要填写奖励 ID

## 服务接口

运行时通过接口保持解耦：

- `IQuestSaveStore`：任务存档读写
- `IQuestItemService`：道具数量查询和消耗
- `IQuestTimeProvider`：时间来源

项目已有实现：

- `YangQuestSaveStore`
- `BagQuestItemService`
- `SystemQuestTimeProvider`

需要测试时可使用 `QuestMemorySaveStore` 和 `QuestMemoryItemService`。

## 查询和事件

```csharp
QuestRuntime runtime = manager.GetQuest("quest_001");
QuestData config = manager.GetStaticData("quest_001");
List<QuestRuntime> allQuests = manager.GetAllQuests();
```

管理器事件：

- `QuestChanged`：任务状态变化
- `ObjectiveChanged`：目标进度变化
- `RewardIssued`：奖励待发放
- `QuestReset`：每日任务重置
- `DataLoaded`：任务配置注册完成

## 注意事项

1. `QuestData.Id` 必须唯一且稳定，修改后会被视为新任务并无法匹配原存档。
2. 前置任务列表填写任务稳定 ID，不是目标或条件的内部键。
3. 目标和条件 ID 由系统按列表索引生成，发布后请保持列表顺序稳定。
4. 事件监听需要在 `OnDisable` 中取消订阅，避免重复回调。
5. 奖励事件只表示需要发放奖励，实际发放由业务层完成。
6. 中文 Inspector 使用 Unity 原生编辑器绘制，不依赖 Odin 的编辑器兼容性。
