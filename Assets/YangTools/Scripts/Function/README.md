# Function 功能模块导航

本目录为 `Assets/YangTools/Scripts/Function` 的运行时功能与示例集合。各模块的用途、依赖、最小接入和当前限制见对应 README；这些部件并非全部相互独立，接入前先确认项目已有的单例、存档、UGUI、TMP、Odin、DOTween 与 UniTask 依赖。

## 模块

| 目录 | 主要内容 |
| --- | --- |
| [Adaptive](Adaptive/README.md) | 旧版 Text 字号缩小适配 |
| [BagManager](BagManager/README.md) | 道具数量缓存与本地存档 |
| [Battle](Battle/README.md) | 角色属性 Buff 技能 连招 武器和投射物演示 |
| [Common](Common/README.md) | 按钮 特效生命周期 残影 走马灯与翻译 |
| [CreateInMesh](CreateInMesh/README.md) | 按三角形面积在网格表面抽样生成 |
| [CustomText](CustomText/README.md) | TMP 注音标签与逐字淡入 |
| [Extend](Extend/README.md) | Task 与 UniTask 的取消等待扩展 |
| [GizmosManager](GizmosManager/README.md) | Scene 视图调试图形 |
| [HpManager](HpManager/README.md) | 世界目标血条 护盾 状态与增益视图 |
| [MageTower](MageTower/README.md) | 法术序列组合的逻辑模拟 |
| [MeshNormalSetter](MeshNormalSetter/README.md) | 网格副本的径向法线混合 |
| [MonsterSpawner](MonsterSpawner/README.md) | 连续和波次怪物生成 |
| [NetIcon](NetIcon/README.md) | 下载网络图片并显示到 Image |
| [NoitaStyleSpells](NoitaStyleSpells/README.md) | 独立法杖 修饰法术 弹体生命周期与对象池 |
| [Parabola](Parabola/README.md) | 抛物线位置计算与移动示例 |
| [PowerSystem](PowerSystem/README.md) | 体力存档 消耗 恢复和 UI |
| [RouletteWheel](RouletteWheel/README.md) | UGUI 转盘 条目与动画 |
| [RuntimeLogging](RuntimeLogging/README.md) | Unity 全局日志等级筛选 |
| [SceneManager](SceneManager/README.md) | 异步场景加载与进度通知 |
| [SelectPoint](SelectPoint/README.md) | 网格拾取 高亮与吸附示例 |
| [TextAdaptiveImage](TextAdaptiveImage/README.md) | TMP 布局首选宽度上限 |
| [TextColor](TextColor/README.md) | TMP 逐字符循环配色 |
| [TMPJump](TMPJump/README.md) | TMP 字符顶点与淡入淡出动画 |
| [UICustom](UICustom/README.md) | 嵌套滚动 圆角图形与 Graphic 扩展入口 |

## 接入与演示

优先复用模块已有的组件和配置，在 Inspector 绑定引用后再调用公开入口。目录内 Prefab 是参考对象；独立演示场景与操作说明见 [Battle 打击感](Battle/HitFeelDemo/README.md) 和 [Battle 投射物模式](Battle/ProjectilePatternDemo/README.md)。其他目录是否包含演示资源，以各自 README 列出的实际文件为准。

Battle、NoitaStyleSpells 与 MageTower 是不同的战斗或法术方案，具有同名 Bullet 等类型时应区分命名空间和配置来源，不能直接互换。部分模块保留模板入口或历史限制，README 已按当前代码说明，未将尚未实现的部分列为可用能力。

## 注释约定

- 类、结构、枚举、接口及方法使用多行 XML 文档注释，说明职责和行为；参数含义或返回约定不直观时补充 param、returns 等标签。
- 字段使用简短中文 `//` 注释；内部逻辑只在有必要解释条件、坐标空间或限制时添加说明。
- 保留版权、历史标识和 TODO。文档更新不更改运行逻辑、字段名、序列化引用或 Inspector 配置。

本说明仅覆盖当前 Scripts/Function 目录，不包含同项目其他 Function 路径。
