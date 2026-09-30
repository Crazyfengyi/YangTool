# SceneManager

`GameSceneManager` 是基于 `MonoSingleton` 的场景切换管理器，提供异步加载进度回调和渐进式加载协程入口。

```csharp
GameSceneManager.Instance.SetOnProgress(new LoadOnProgress
{
    OnLoading = progress => UpdateLoadingUI(progress)
});
GameSceneManager.Instance.Load("Level01");
```

`Load` 默认使用 `LoadSceneMode.Single` 并自动跳过加载界面（具体跳转逻辑按配置）；需要保留当前场景时传入 `LoadSceneMode.Additive`。Build Settings/Addressable 场景配置必须包含目标场景。`SetOnProgress` 的回调需在发起加载前设置；界面对象销毁时应解除或替换旧回调。
