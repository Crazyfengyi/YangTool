# YangTool 项目资源导航

YangTool 是一个 Unity 工具与功能示例项目，包含可复用的基础框架、游戏功能、编辑器工具和独立玩法原型。各示例有自己的入口和配置，接入时应先阅读对应模块的说明。

## 开发环境与主要依赖

- Unity：`6000.5.0f1`，以 [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) 为准。
- 渲染：URP `17.0.3`；界面主要使用 UGUI 和 TextMesh Pro。
- 资源管理：YooAsset `2.3.19`；另使用 Cinemachine、Input System、Timeline 等 Unity 包。
- 编辑器扩展：Odin Inspector `3.3.1.13`，插件安装在 `YangTools/Plugins/Sirenix_Odin`。
- 包依赖以 [Packages/manifest.json](Packages/manifest.json) 为准；部分第三方插件直接随 Assets 保存。

## Assets 主要结构

```text
Assets/
├── YangTools/         工具框架、可复用功能、编辑器扩展和配套资源
├── UserTest/          独立示例、玩法原型和测试场景
├── Art/               项目美术资源
├── Plugins/           第三方插件与示例资源
├── Resources/         Assets 根目录下的 Resources 资源
├── Settings/          项目渲染等配置资源
├── StreamingAssets/   随应用部署的资源文件
├── TextMesh Pro/      TMP 配套资源
├── Gizmos/            编辑器 Gizmos 资源
└── _Recovery/         场景恢复快照
```

| 目录 | 主要内容 |
| --- | --- |
| [YangTools](Assets/YangTools/) | 框架与工具主体，运行时代码、编辑器代码和资源分目录保存 |
| [UserTest](Assets/UserTest/) | ThroughWorld、FruitTowerDemo、导弹、商店、签到等示例，以及个人测试内容 |
| [Art](Assets/Art/) | 图片、字体、模型、特效、Shader、天空盒和应用图标 |
| [Plugins](Assets/Plugins/) | 项目随附的第三方库、组件及其示例；与 YangTools 内的插件目录共同组成依赖 |
| [Resources](Assets/Resources/) | 此处及各模块内同名目录用于 Resources 资源加载；定位资源时需同时检查模块目录 |
| [Settings](Assets/Settings/) | Assets 内的配置资产；Unity 项目级设置另存于根目录 `ProjectSettings` |
| [StreamingAssets](Assets/StreamingAssets/) | 原样部署的资源文件，具体用途结合资源加载和构建流程查看 |
| [TextMesh Pro](Assets/TextMesh%20Pro/) | TMP 必需或配套资源；项目自用字体还位于 `Art/Fonts` |
| [Gizmos](Assets/Gizmos/) | 编辑器场景辅助显示所用资源 |
| [_Recovery](Assets/_Recovery/) | 恢复内容，查找正式游戏入口时优先查看各示例场景 |

## YangTools 框架与功能

### 基础框架：Scripts/Core

[Core](Assets/YangTools/Scripts/Core/) 提供项目通用基础能力：

- **启动与资源**：`InitPatchLogic`、`YooAsset`、`ResourceManager`，负责初始化、资源包补丁流程及资源加载封装。参见[初始化与热更新流程](Assets/YangTools/Scripts/Core/InitPatchLogic/README.md)。
- **服务与配置**：`YangToolsManager`、`TableManager`、`LanguageManager`、`SDK`，包含管理入口、表数据、多语言和平台接入。
- **运行支持**：`YangEvent`、`YangFSM`、`YangTimer`、`YangCoroutine`、`YangTask`、`YangTaskTree`、`YangObjectPool`，包含事件、状态机、计时、协程、任务与对象池。
- **界面与交互**：`YangUGUI`、`RedDotSystem`、`YangGuideSystem`、`YangInteractionSystem`、`YangMiniMap`，提供 UI、红点、引导、交互和小地图相关组件。
- **其他能力**：`YangAudio`、`YangSaveData`、`YangDownLoad`、`YangEncrypt`，涵盖音频、存档、下载和加密工具。

### 可复用功能：Scripts/Function

[Scripts/Function](Assets/YangTools/Scripts/Function/) 存放相对独立的功能模块，各模块目录中的 README 是进一步了解实现和接入方式的入口。

- [Battle](Assets/YangTools/Scripts/Function/Battle/)：角色数值、Buff、技能、连招、武器及其 ScriptableObject 配置。当前目录按 `1_RoleBase`、`2_BuffBase`、`3_SkillBase`、`4_Weapon`、`5_SO` 划分，示例位于 `HitFeelDemo` 和 `Tests`。
- `BagManager`、`HpManager`、`MonsterSpawner`、`PowerSystem`：背包、血量、生成器和体力相关功能。
- `Parabola`、`SelectPoint`、`CreateInMesh`：抛物线轨迹、选点和网格内生成等工具。
- `MageTower`、`NoitaStyleSpells`：法术组合相关实现。
- `Adaptive`、`UICustom`、`CustomText`、`TextAdaptiveImage`、`TextColor`、`TMPJump`：界面适配、UI 与文本表现。
- `RuntimeLogging`、`SceneManager`、`Common`、`Extend` 等：日志、场景及通用扩展功能。

### 编辑器、配表与独立系统

| 目录 | 用途 |
| --- | --- |
| [EditorStudio](Assets/YangTools/EditorStudio/) | 字体、图片、预制体、构建、截图、存档等编辑器工具；常用入口位于 Unity 的 `YangTools` 菜单 |
| [LubanData](Assets/YangTools/LubanData/) | Luban 相关运行库及生成数据，结合 Core 的表管理模块使用 |
| [DialogSystem](Assets/YangTools/DialogSystem/) | 对话数据和节点图编辑工具 |
| [QuestSystem](Assets/YangTools/QuestSystem/README.md) | 任务系统及其集成、存档与测试说明 |
| [Function/AutoBuildSystem](Assets/YangTools/Function/AutoBuildSystem/README.md) | 自动构建配置与运行；菜单入口为 `YangTools → 自动构建 → 配置与运行` |
| [Function/IrregularPuzzle](Assets/YangTools/Function/IrregularPuzzle/) | 不规则拼图相关内容 |
| [ArtRes](Assets/YangTools/ArtRes/) | 框架功能配套美术资源 |
| [AssetBunds](Assets/YangTools/AssetBunds/) | 模块资源目录，具体收集与加载方式结合 YooAsset 配置查看 |
| [Resources](Assets/YangTools/Resources/) | 框架模块的 Resources 资源 |
| [Plugins](Assets/YangTools/Plugins/) | 框架随附插件，包含 Odin 等依赖 |

注意：`YangTools/Function` 与 `YangTools/Scripts/Function` 是两个实际存在的目录，查找功能时需区分。

## 示例与测试入口

| 示例 | 入口 | 内容与使用方式 |
| --- | --- | --- |
| ThroughWorld | [Init.unity](Assets/UserTest/AllProjects/ThroughWorld/Secenes/Init.unity) | 从初始化场景进入资源与游戏启动流程；同目录包含 GameEntry、GameMain 和关卡场景，运行前检查资源包配置 |
| 打击感演示 | [HitFeelDemo.unity](Assets/YangTools/Scripts/Function/Battle/HitFeelDemo/HitFeelDemo.unity) | 技能投射物、命中及屏幕反馈；操作参见[示例说明](Assets/YangTools/Scripts/Function/Battle/HitFeelDemo/README.md) |
| 连招测试对象 | [Demmo.prefab](Assets/YangTools/Scripts/Function/Battle/Tests/Demmo.prefab) | 将预制体放入测试场景，使用鼠标左键触发连招；技能与连招配置位于 Battle 的 `5_SO`，测试时结合 Console 事件日志观察 |
| 追踪导弹 | [HomingMissileDemo.unity](Assets/UserTest/HomingMissileDemo/HomingMissileDemo.unity) | 导弹相关独立演示，具体操作与参数查看同目录控制脚本和 Inspector |
| 水果塔 | [FruitTowerDemo.unity](Assets/UserTest/AllProjects/FruitTowerDemo/Scenes/FruitTowerDemo.unity) | 水果塔玩法原型 |
| EndlessWorlds | [EndlessWorld.unity](Assets/UserTest/AllProjects/EndlessWorlds/Scene/EndlessWorld.unity) | 独立世界场景示例 |
| 商店 | [ShopExample.unity](Assets/UserTest/ShopSystem/Examples/ShopExample.unity) | 商店模块示例，参见[模块说明](Assets/UserTest/ShopSystem/README.md) |
| 签到 | [模块说明](Assets/UserTest/SignInSystem/README.md) | 七日签到预制体及奖励配置，按说明将管理器和窗口接入场景 |

其他测试内容位于 `UserTest/MyTest`、`UserTest/Scenes`、`MapEditor`、`SoundSystem` 等目录。上表用于定位入口，不代表所有示例已在当前环境完成运行验收。

## 使用与维护

1. 使用上方记录的 Unity 版本打开项目，等待依赖导入与脚本编译，再选择需要的示例入口。
2. 先确认该模块的场景、预制体、SO 配置、资源和插件引用齐全，再接入自己的游戏流程。运行预制体示例时需准备相机、灯光及其所需组件。
3. 编辑器工具优先从 `YangTools` 菜单查找；模块详细参数和操作以其 README、Inspector 和代码为准。
4. 资源加载既包含 Resources 方式，也包含 YooAsset 封装；修改目录或导出模块前应核对实际引用和资源收集配置。
5. 保留资源 `.meta` 文件以维护 GUID 引用；修改第三方插件前保留原版及必要配置，避免同时导入重复程序集。
6. 当前 Odin 3.3.1.13 使用针对 Unity 6000.5 的本地兼容修补，脚本为 [Patch-OdinUnity6.ps1](Tools/OdinCompatibility/Patch-OdinUnity6.ps1)。它修正 IMGUI 内部成员迁移引用；重新覆盖插件时需核对修补状态，这不等同于插件全部功能已获得官方兼容保证。
7. 新增功能和示例时，优先在模块内补充说明，并更新本页的目录或入口链接。


### 投射物模式演示

Battle 新增 [ProjectilePatternDemo](Assets/YangTools/Scripts/Function/Battle/ProjectilePatternDemo/README.md)，包含 7 种发射排列、10 种飞行轨迹和三个生命周期效果列表。场景入口为 [ProjectilePatternDemo.unity](Assets/YangTools/Scripts/Function/Battle/ProjectilePatternDemo/ProjectilePatternDemo.unity)。
