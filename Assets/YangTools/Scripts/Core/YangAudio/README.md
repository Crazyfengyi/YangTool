# 音频管理

`YangAudioManager` 是 YangTools 模块化音频服务，集中管理背景音乐、对话、普通音效和绑定场景物体的世界音效，并支持音量、静音、渐变暂停与播放句柄控制。

## 主要文件

- `YangAudioManager.cs`：创建 AudioSource、异步加载音频片段并管理播放状态。
- `AudioHandle`：标识一次播放，可独立停止、暂停或恢复。

## 使用

```csharp
AudioHandle handle = YangAudioManager.Instance.PlayBGM("Audio/BGM/Main");
// 需要停止该次播放时
handle.Stop();
```

音频管理器由 `YangToolsManager` 初始化。项目需提供 `Resources/Audios/AudioMixer`，并配置 `BGM`、`Dialogue`、`Sound` 等混音组；音频名称需符合 `ResourceManager` 使用的资源地址。其他播放入口包括 `PlaySingleSound`、`PlaySoundAudio` 和 `PlayWorldSound`。临时播放结束后及时处理句柄，避免业务持有过期状态。
