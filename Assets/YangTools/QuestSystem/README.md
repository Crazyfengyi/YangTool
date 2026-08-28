# QuestSystem 任务系统

QuestSystem 是基于 `ScriptableObject` 的任务运行时模块，负责任务状态、目标条件、进度、每日刷新、存档接口和奖励事件。系统拆分为独立核心与可选项目适配层，整个目录可以直接复制到没有 YooAsset、BagMgr、YangSaveDataManager 或 YangEventGroup 的新项目中。

## 目录结构

```text
Assets/YangTools/QuestSystem/
├─ Runtime/
│  ├─ QuestSystem.Runtime.asmdef  独立核心程序集
│  ├─ QuestData.cs                任务配置数据
│  ├─ QuestEvents.cs              任务事件和状态
│  ├─ QuestRuntime.cs             运行时进度逻辑
│  ├─ QuestManager.cs             任务管理器单例
│  ├─ QuestServices.cs            服务接口和内存实现
│  ├─ QuestSaveModels.cs          独立存档模型
│  └─ QuestEventMessageBase.cs    核心事件基类
├─ Integration/
│  └─ QuestProjectAdapters.cs      可选 Yang 存档、BagMgr、YangEventGroup、YooAsset 适配器
├─ Script/
│  └─ QuestManagerBootstrap.cs     双模式启动引导器
├─ Editor/QuestSystem/             中文任务 Inspector
└─ Data/                           任务资源和管理器预制体
```

### 新项目最快上手

1. 复制 `QuestSystem` 目录到新项目的 `Assets` 下，不要定义 `YANGTOOLS_QUEST_INTEGRATION`。
2. 将 `Data/QusetManager.prefab` 拖入首个场景（或创建对象并添加 `QuestManager`、`QuestManagerBootstrap`）。
3. 通过 `Assets/Create/Game/Quest/QuestData` 创建任务，填写唯一 `Id`、标题、目标/条件和奖励；`OnLineTime` 的目标数量按秒填写。
4. 把任务资源拖到 Bootstrap 的“通用模式本地任务”列表，运行即可。Bootstrap 会自动注入内存存档和空道具服务。
5. 业务代码通过 `QuestManager.Instance` 调用 `AcceptQuest`、`ReportProgress`、`CompleteQuest`、`ClaimReward`；监听 `QuestChanged` 或 `DataLoaded` 更新界面。

不需要存档、背包或 YooAsset 配置即可运行。后续要接入自己的系统时，再按下方“注入接口”实现服务并在注册任务前注入。

## 存档兼容

核心层只依赖 `QuestSaveItem`、`QuestSaveObjectiveItem` 和 `QuestSaveConditionItem`。
集成层会把它们映射到项目存档类型，任务状态、目标索引和条件索引结构保持不变；在线时长字段统一按秒读写。
切换秒制后请清理或重置旧的在线时长存档。目标和条件不需要手动填写 ID，运行时按列表索引生成内部键，已上线任务不要随意调整列表顺序。

## 基本用法

```csharp
QuestManager manager = QuestManager.Instance;
manager.Initialize(new[] { questData });
manager.AcceptQuest("quest_001");
manager.ReportProgress(new QuestProgressEvent(QuestProgressEventType.Kill, "enemy_001"));
manager.CompleteQuest("quest_001");
manager.ClaimReward("quest_001");
```

`QuestManager` 是场景单例，默认跨场景保留。`Update` 会自动驱动时间和在线时长条件。`DataLoaded` 在当前批次任务注册完成后触发。

`QuestData` 的“默认激活任务”用于控制任务注册时的初始状态。启用后，任务在前置任务满足时直接进入 `Active`（进行中），不需要额外调用 `AcceptQuest`；前置任务未满足时仍保持 `Locked`，满足后会自动激活。关闭时任务默认进入 `Available`（可接取）。已有 `Active`、`Completed` 或 `Rewarded` 存档状态不会被降级。

`OnLineTime` 条件的“目标数量”单位为秒，例如 60 表示在线 1 分钟。进度事件中的 `Value` 和运行时存档字段 `onlineTimeSeconds` 也统一使用秒。

默认任务窗口的按钮会根据任务状态执行不同操作：`Locked` 显示“锁定”，`Available` 显示“接取任务”，`Active` 显示“进行中”；当 `AutoComplete` 关闭且目标条件满足时显示“确认完成”，`Completed` 显示“可领取”，`Rewarded` 显示“已领取”。

## 注入接口（自定义集成必读）

接口定义位于 `Runtime/QuestServices.cs`。这里是“实现接口”，不是必须继承某个 `MonoBehaviour`。使用 Bootstrap 的默认模式无需编写实现类；只有在替换默认服务或接入项目系统时才需要编写实现类：

| 接口 | 是否必须 | 用途 | 必须提供的成员 |
| --- | --- | --- | --- |
| `IQuestSaveStore` | 管理器运行时必须有；默认由 Bootstrap 提供 | 读取、创建、清理和保存任务进度 | `GetQuest`、`GetOrCreateQuest`、`Clear`、`MarkDirty` |
| `IQuestItemService` | 使用 `ItemNum` 条件时实现；否则可用内置空服务 | 查询和消耗道具 | `GetItemCount`、`HasItem`、`TryConsume` |
| `IQuestTimeProvider` | 否 | 提供 UTC 秒数和本地日期键 | `UtcNowSeconds`、`LocalDateKey` |

`ConfigureServices` 必须传入非空的 `IQuestSaveStore`；`IQuestItemService` 可以传 `null`，系统会自动使用 `NullQuestItemService.Instance`。不使用道具条件时也可显式传入该空服务；不提供时间服务时使用系统时间。Bootstrap 已经自动配置服务时，不要重复覆盖。

最小自定义注入示例（必须在注册任务前调用）：

```csharp
public sealed class MyQuestSaveStore : IQuestSaveStore { /* 实现 4 个成员 */ }
public sealed class MyQuestItemService : IQuestItemService { /* 实现 3 个成员 */ }

QuestManager manager = QuestManager.Instance;
manager.ConfigureServices(new MyQuestSaveStore(), new MyQuestItemService());
manager.Initialize();
manager.RegisterQuests(questDatas);
manager.CompleteRegistration();
```

如果需要自定义时间，再实现 `IQuestTimeProvider` 并作为第三个参数传入。服务配置完成并开始注册后不能再替换；核心存档类型使用 `QuestSaveItem`、`QuestSaveObjectiveItem` 和 `QuestSaveConditionItem`，项目适配层负责与现有存档类型互转。

## 奖励和事件

管理器只发布 `RewardIssued`，不直接修改金币或背包。`QuestRewardData.TargetKey` 用于填写道具 ID 或业务自定义目标键，奖励列表不需要额外 ID。

任务事件包括 `QuestChanged`、`ObjectiveChanged`、`OnlineTimeProgressed`、`RewardIssued`、`QuestReset` 和 `DataLoaded`。其中 `OnlineTimeProgressed` 每秒发送在线时长增量，`OnLineTime` 条件的目标数量、事件数值和存档字段统一使用秒。核心事件基类不依赖项目事件系统，集成模式下由适配器转发到 YangEventGroup。

## 默认任务窗口

`DefaultWindow/Window/TaskWindow.prefab` 的 `TaskWindow` 直接继承 `MonoBehaviour`，只使用 Unity 原生的 `Toggle`、`ToggleGroup`、`Button`、`ScrollRect` 和 `Content` 容器，不依赖 YangUGUI、`UICustomToggle`、`UICustomButton` 或 EnhancedScroller。通过 `Open()` 和 `Close()` 控制窗口显隐；打开窗口或切换分类时会直接实例化当前分类的全部 `TaskNode`，窗口会在刷新前销毁旧节点，并按任务状态和进度排序后重新生成。

`TaskNode` 的本地化、广告和背包物品视图均按可选服务处理。缺少项目专属服务时仍可显示任务标题、进度并调用核心 `ClaimReward`；具备项目 UI 资源时，可在预制体中绑定对应按钮、文本和奖励视图以启用完整表现。

如果只使用 QuestSystem 核心而不需要默认窗口，可以不引用 `DefaultWindow` 目录；核心运行时不依赖任何 UI 或第三方滚动列表插件。

## 注意事项

1. `QuestData.Id` 必须全局唯一且稳定，用于前置任务和存档索引。
2. 目标和条件 ID 由系统按列表索引生成，不是配置必填项。
3. 事件监听应在 `OnDisable` 中取消，避免重复回调。
4. 奖励事件只表示待发放奖励，实际发放由业务层完成。
