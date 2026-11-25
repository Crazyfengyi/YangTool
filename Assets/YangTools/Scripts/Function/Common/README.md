# Common 通用组件

## 功能与入口

本目录收纳按钮交互、特效生命周期、残影、走马灯与翻译辅助。

| 入口 | 内容 |
| --- | --- |
| [Btn](Btn) | BtnExtension 单击 双击 长按 TemplateBtn 外观切换 YangToggleGroup 索引选择 |
| [CommonBtn](CommonBtn) | UICustomButton UICustomSlider UICustomToggle |
| [EffectAutoDestory.cs](EffectAutoDestory.cs) | 非循环粒子播放完成后的物体销毁 |
| [LoopEffectDestory.cs](LoopEffectDestory.cs) | 停止循环粒子后等待消散并销毁 |
| [WaitLineDestory.cs](WaitLineDestory.cs) | 等待拖尾消散后销毁 |
| [GhostTrail.cs](GhostTrail.cs) | 蒙皮网格采样与残影绘制 |
| [TextWalkingLantern.cs](TextWalkingLantern.cs) | 旧版 UGUI Text 走马灯 |
| [BaiduTranslate.cs](BaiduTranslate.cs) | 百度翻译请求与响应数据封装 |

## 依赖与配置

UI 依赖 UGUI 和 EventSystem，部分组件使用 DOTween、TMP、Odin 及项目音频服务。特效依赖粒子或 TrailRenderer；残影要求 SkinnedMeshRenderer 以及支持透明度属性的材质。翻译请求依赖 LitJson 和网络环境，调用方需要配置自己的 App ID 与密钥。

## 最小接入

在按钮上配置 `BtnExtension` 的单击、双击或长按开关与事件。`TemplateBtn` 需要同时配置两种外观按钮；组选择可通过 `toggleGroup.SetAction(index => SelectTab(index))` 接入业务。

循环特效不再使用时调用 `LoopEffectDestroy.DestoryLoopEffect()`，让现存粒子消散后再销毁。历史文件名使用 `Destory` 拼写，引用类与方法时以源码为准。

## 示例与限制

[走马灯效果.prefab](走马灯效果.prefab) 为走马灯参考。`Btn/Editor` 仅负责编辑器绘制。`TemplateBtn.SetClick` 当前和 `SetDoubleClick` 一样写入双击事件，接入单击时应检查实际绑定；`UICustomToggle` 的点击回调不会自动反转 `isOn`。自动销毁特效不处理循环粒子的正常结束逻辑。百度翻译封装使用同步 HTTP 请求，不要据此假定已具备异步、重试或跨平台联网保障。
