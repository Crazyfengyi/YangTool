# SceneManager 场景加载

## 功能与入口

[GameSceneManager.cs](GameSceneManager.cs) 通过 Unity `SceneManager.LoadSceneAsync` 加载场景，并提供开始、进度和完成回调。`IsAutoSkip` 控制异步操作是否允许激活新场景。

## 依赖与配置

依赖项目 `MonoSingleton`。目标场景应加入当前构建的场景列表；本实现没有通过 Addressables 或 YooAsset 加载场景。先配置回调，再发起加载。

## 最小接入

```csharp
YangTools.GameSceneManager.Instance.SetOnProgress(new YangTools.LoadOnProgress
{
    OnLoading = progress => UpdateLoadingUI(progress)
});
YangTools.GameSceneManager.Instance.Load("Level01");
```

默认 Single 模式并自动激活，可传入 Additive 保留已有场景。关闭自动激活后需由调用方适时将 `IsAutoSkip` 设置为 true。

## 示例与限制

本目录没有演示场景。回调由管理器持有，界面结束使用时应解除或替换引用。`DoGentleSceneLoad` 会主动销毁当前场景根对象、卸载场景、回收未使用资源后加载新场景，调用方需准备好过渡流程。

编辑器条件下的 `Loading(string sceneName)` 仅 yield 一帧，用于隐藏动画事件中的同名选项；实际加载使用无参 `Loading()` 协程，不要误用占位重载。
