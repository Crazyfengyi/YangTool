# QuestSystem 任务系统

QuestSystem 是一个基于 Unity `ScriptableObject` 的任务运行时模块，提供任务配置、目标条件、进度事件、每日刷新、存档、奖励发放和任务 UI 接口。

系统核心不依赖 `YooAsset`、`BagMgr`、`YangSaveDataManager` 或 `YangEventGroup`。需要接入项目现有系统时，启用 `YANGTOOLS_QUEST_INTEGRATION` 并使用 `Integration` 目录中的适配器。

## 目录结构

```text
Assets/YangTools/QuestSystem/
├─ Runtime/
│  ├─ Core/
│  │  ├─ QuestManager.cs          任务管理器
│  │  └─ QuestRuntime.cs           任务运行时和条件运行时
│  ├─ Data/
│  │  └─ QuestData.cs              任务配置和目标配置
│  ├─ Conditions/
│  │  └─ QuestConditions.cs        多态条件和组合条件
│  ├─ Events/
│  │  ├─ QuestEventBus.cs          强类型任务事件总线
│  │  ├─ QuestEventMessageBase.cs  项目事件扩展基类
│  │  └─ QuestEvents.cs             进度事件和任务事件
│  ├─ Rewards/
│  │  └─ QuestRewards.cs            多态奖励
│  ├─ Services/
│  │  └─ QuestServices.cs           存档 道具 奖励 时间服务
│  ├─ Save/
│  │  └─ QuestSaveModels.cs         核心存档模型
│  └─ QuestSystem.Runtime.asmdef    独立核心程序集
├─ Integration/
│  ├─ Save/
│  │  └─ YangQuestSaveStore.cs    项目存档适配器
│  ├─ Items/
│  │  └─ BagQuestItemService.cs   BagMgr 道具适配器
│  ├─ Rewards/
│  │  └─ BagQuestRewardService.cs BagMgr 奖励适配器
│  ├─ Events/
│  │  └─ YangQuestEventBridge.cs  项目事件适配器
│  └─ Loading/
│     └─ YooAssetQuestDataLoader.cs YooAsset 配置加载器
├─ Script/
│  └─ QuestManagerBootstrap.cs    任务系统启动引导器
├─ Editor/QuestSystem/
│  └─ QuestDataEditor.cs          中文任务配置 Inspector
├─ DefaultWindow/                 默认任务窗口和任务节点
└─ Data/                          示例任务资源和管理器预制体
```

## 快速开始

### 使用 Bootstrap

1. 将 `Data/QusetManager.prefab` 拖入场景，或创建对象并添加 `QuestManager` 和 `QuestManagerBootstrap`。
2. 通过 `Assets/Create/Game/Quest/QuestData` 创建任务资源。
3. 在任务资源中配置任务 ID、标题、目标、根条件和奖励。
4. 未启用 `YANGTOOLS_QUEST_INTEGRATION` 时，将任务资源拖入 Bootstrap 的“通用模式本地任务”列表。
5. 运行后，Bootstrap 会自动使用内存存档、空道具服务、系统时间服务和内存奖励服务。

默认模式适合开发和功能验证。内存存档不会写入持久化文件，重新运行后进度会丢失。

### 手动初始化

```csharp
QuestManager manager = QuestManager.Instance;
manager.ConfigureServices(
    new QuestMemorySaveStore(),
    NullQuestItemService.Instance,
    SystemQuestTimeProvider.Instance,
    new QuestMemoryRewardService());

manager.Initialize(new[] { questData });

manager.AcceptQuest(questData.Id);
manager.ReportProgress(new QuestProgressEvent(
    QuestProgressEventType.Kill, "slime", amount: 1f));

// AutoComplete 关闭时需要手动确认当前目标
manager.CompleteQuest(questData.Id);
manager.ClaimReward(questData.Id);
```

`Initialize(IEnumerable<QuestData>)` 会注册任务并完成当前批次加载。也可以先调用 `Initialize()`，再调用 `RegisterQuest`、`RegisterQuests` 和 `CompleteRegistration`。

## 任务配置结构

`QuestData` 使用 Unity 原生 `[SerializeReference]` 保存具体的 Condition 和 Reward 子类。

每个 `QuestObjectiveData` 只有一个根条件：

```csharp
[SerializeReference]
public Condition Condition;
```

不再使用目标条件列表，也不再使用额外的“条件组合方式”枚举。需要多个条件时，将根条件设置为 `AndCondition` 或 `OrCondition`，然后在其 `Children` 中添加子条件。组合条件可以继续嵌套。

示例结构：

```text
AndCondition
├─ KillCondition       击杀 3 个 slime
└─ OrCondition         任意满足其一
   ├─ TalkCondition    与 merchant 对话
   └─ ReachLocation    到达 town
```

编辑器中的条件和奖励类型菜单使用中文显示。新增自定义类型需要满足以下条件：

- 继承 `Condition` 或 `Reward`
- 添加 `[Serializable]`
- 提供无参数构造函数
- 条件在 `Initialize` 中订阅事件并在 `Dispose` 中取消订阅
- 条件变化时调用 `MarkChanged` 或 `Complete`

## 内置条件

| 类型 | 说明 | 主要配置 |
| --- | --- | --- |
| `KillCondition` | 击杀指定怪物 | `MonsterId`、`TargetCount` |
| `CollectCondition` | 收集指定道具 | `ItemId`、`TargetCount` |
| `TalkCondition` | 与指定 NPC 对话 | `NpcId` |
| `ReachLocationCondition` | 到达指定地点 | `LocationId` |
| `ItemNumCondition` | 背包已有指定数量道具 | `ItemId`、`TargetCount` |
| `ProgressCondition` | 监听通用进度事件 | `EventType`、`TargetId`、`TargetCount` |
| `CustomEventCondition` | 监听自定义事件 | `TargetId`、`TargetCount` |
| `AdsCondition` | 完成广告次数 | `TargetId`、`TargetCount` |
| `PassNumCondition` | 达到通关数量 | `TargetId`、`TargetCount` |
| `TimeCondition` | 累计真实时间 | `TargetCount`，单位为秒 |
| `OnlineTimeCondition` | 累计在线时间 | `TargetCount`，单位为秒 |
| `AndCondition` | 所有子条件满足 | `Children` |
| `OrCondition` | 任一子条件满足 | `Children` |

`TimeCondition` 和 `OnlineTimeCondition` 使用秒作为单位，例如目标数量填写 `60` 表示 1 分钟。

## 自定义条件示例

条件可以直接针对自己的事件实现逻辑：

```csharp
using System;

[Serializable]
public sealed class BossKilledCondition : CountCondition
{
    public string BossId;
    public new int TargetCount = 1;

    protected override int GetConfiguredTargetCount() => TargetCount;

    public override void Initialize()
    {
        QuestEventBus.OnMonsterKilled += OnMonsterKilled;
    }

    public override void Dispose()
    {
        QuestEventBus.OnMonsterKilled -= OnMonsterKilled;
    }

    private void OnMonsterKilled(string monsterId)
    {
        if (string.Equals(monsterId, BossId, StringComparison.Ordinal))
        {
            AddCount(1f);
        }
    }
}
```

运行时会复制配置中的 Condition，不会修改 QuestData 资源本身。只有当前激活目标的根条件会初始化和监听事件；目标完成后会释放旧条件并初始化下一个目标。

## 任务事件

普通业务事件可以直接发布到强类型事件总线：

```csharp
QuestEventBus.PublishMonsterKilled("slime");
QuestEventBus.PublishItemAdded("herb", 2);
QuestEventBus.PublishNpcTalked("merchant");
QuestEventBus.PublishLocationEntered("town");
```

通用事件使用 `QuestProgressEvent`：

```csharp
QuestEventBus.Publish(new QuestProgressEvent(
    QuestProgressEventType.Custom,
    "daily_login",
    amount: 1f));
```

`QuestManager.ReportProgress` 与直接调用 `QuestEventBus.Publish` 使用同一条分发链。`QuestEventBus` 提供以下事件：

- `OnMonsterKilled`
- `OnItemAdded`
- `OnNpcTalked`
- `OnLocationEntered`
- `OnProgressReported`

项目集成模式下，`YangQuestEventBridge` 负责将已有的 Yang 事件转换为 `ReportProgress` 调用；业务层也可以直接使用 `QuestEventBus`。

## QuestManager 生命周期

任务管理器默认是跨场景单例，主要流程如下：

1. `ConfigureServices` 注入服务
2. `Initialize` 初始化管理器
3. `RegisterQuest` 或 `RegisterQuests` 注册任务配置
4. `CompleteRegistration` 完成任务加载
5. `AcceptQuest` 接取可接取任务
6. 通过 `ReportProgress` 或 `QuestEventBus` 发布进度
7. `AutoComplete` 开启时自动完成目标，否则调用 `CompleteQuest`
8. 任务进入 `Completed` 后调用 `ClaimReward`

`QuestManager.Update` 会自动驱动真实时间和在线时长条件。应用进入后台后不会累计在线时长，回到前台后继续累计。

任务目标按顺序执行。后续目标不会提前接收事件或累计进度。

## 服务注入

接口定义在 `Runtime/Services/QuestServices.cs`：

| 接口 | 用途 | 默认实现 |
| --- | --- | --- |
| `IQuestSaveStore` | 读取和保存任务进度 | `QuestMemorySaveStore` |
| `IQuestItemService` | 查询和消耗道具 | `NullQuestItemService` |
| `IQuestRewardService` | 发放任务奖励 | `NullQuestRewardService` 或 `QuestMemoryRewardService` |
| `IQuestTimeProvider` | 提供 UTC 时间和日期键 | `SystemQuestTimeProvider` |

`ConfigureServices` 必须传入非空的 `IQuestSaveStore`。其他服务可以传 `null`，系统会使用对应的默认服务。服务配置和任务注册开始后不能再替换。

集成适配器当前提供：

- `YangQuestSaveStore`：接入项目任务存档
- `BagQuestItemService`：接入 `BagMgr` 查询和消耗道具
- `BagQuestRewardService`：接入 `BagMgr` 发放道具奖励
- `YangQuestEventBridge`：接入项目事件系统
- `YooAssetQuestDataLoader`：从 YooAsset 加载任务资源

当前项目的金币、现金和经验奖励仍需要由外部奖励服务实现。

## 奖励发放

内置奖励类型：

- `MoneyReward`：现金
- `GoldReward`：金币
- `ExpReward`：经验
- `ItemReward`：道具
- `CustomReward`：自定义奖励

每个奖励独立实现 `Give(IQuestRewardService)`。`ClaimReward` 会先保存 `Rewarded` 状态，再逐项发放奖励，防止回调重入导致重复领奖。单项奖励失败只记录错误并触发 `RewardIssued`，不会自动回滚或重试。

## 存档

核心存档模型包括：

- `QuestSaveItem`
- `QuestSaveObjectiveItem`
- `QuestSaveConditionItem`

组合条件的子条件状态会递归保存，时间条件和在线时长条件均以秒保存。运行时状态由 `QuestRuntime` 写入存档，条件配置资源只作为运行时副本的来源。

当前重构不迁移旧版 QuestData 资源和旧任务存档。升级后请重新配置任务资源，并清理旧存档数据。

## 任务事件回调

`QuestManager` 提供以下事件：

- `QuestChanged`：任务状态变化
- `ObjectiveChanged`：目标进度或完成状态变化
- `OnlineTimeProgressed`：在线时长增量变化
- `RewardIssued`：单项奖励发放结果
- `QuestReset`：任务重置
- `DataLoaded`：当前批次任务配置加载完成

## 默认任务窗口

`DefaultWindow/Window/TaskWindow.prefab` 提供基础任务列表界面。`TaskWindow` 负责按任务类型刷新任务节点，`TaskNode` 负责显示任务状态、进度和奖励。

默认 UI 只依赖 Unity 原生的 `Toggle`、`Button`、`ScrollRect` 和 `Content`。不使用默认 UI 时，可以只保留 `Runtime` 目录。

## 注意事项

1. `QuestData.Id` 必须全局唯一且稳定，用于前置任务和存档索引。
2. 目标只配置一个根 `Condition`，多条件请使用 `AndCondition` 或 `OrCondition` 的 `Children`。
3. 条件必须成对实现事件订阅和取消订阅，避免重复监听或残留监听。
4. `ItemNumCondition` 需要有效的 `IQuestItemService` 才能刷新背包数量。
5. 奖励发放需要有效的 `IQuestRewardService`，否则会通过 `RewardIssued` 上报失败。
6. 自定义 Condition 和 Reward 必须使用 `[Serializable]`，并提供无参数构造函数，才能在原生 SerializeReference Inspector 中创建。
7. 修改 Condition 或 Reward 的字段结构后，需要检查已有 SerializeReference 资源是否需要重新配置。
