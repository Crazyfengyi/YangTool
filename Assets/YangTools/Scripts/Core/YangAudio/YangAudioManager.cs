using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using YangTools.Scripts.Core;
using Object = UnityEngine.Object;

namespace YangTools.Scripts.Core.YangAudio
{
    /// <summary>
    /// 音频播放句柄
    /// </summary>
    public class AudioHandle
    {
        /// <summary>
        /// 音频播放ID
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// 音频管理器引用
        /// </summary>
        public YangAudioManager Manager { get; }

        /// <summary>
        /// 创建音频播放句柄
        /// </summary>
        /// <param name="id">音频播放ID</param>
        /// <param name="manager">音频管理器引用</param>
        public AudioHandle(int id, YangAudioManager manager)
        {
            ID = id;
            Manager = manager;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            Manager?.StopAudio(ID);
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            Manager?.PauseAudio(ID);
        }

        /// <summary>
        /// 恢复播放
        /// </summary>
        public void Resume()
        {
            Manager?.ResumeAudio(ID);
        }
    }

    /// <summary>
    /// 声音管理器
    /// </summary>
    public class YangAudioManager : GameModuleBase
    {
        #region 属性

        //设备一般最多允许32个音源同时播放,优先级参数决定了在超出音源数目时，需要暂时关闭一些不重要的音源，优先播放更重要的音源
        /// <summary>
        /// 背景优先级
        /// </summary>
        private static readonly int BackgroundPriorityDefault = 0;

        /// <summary>
        /// 对话优先级
        /// </summary>
        private static readonly int SinglePriorityDefault = 10;

        /// <summary>
        /// 声音优先级
        /// </summary>
        private static readonly int MultiplePriorityDefault = 20;

        /// <summary>
        /// 世界音效优先级
        /// </summary>
        private static readonly int WorldPriorityDefault = 30;

        /// <summary>
        /// 声音的物体
        /// </summary>
        public static GameObject managerObject;

        /// <summary>
        /// 背景
        /// </summary>
        private static AudioSource bgmAudio;

        /// <summary>
        /// 对话
        /// </summary>
        private static AudioSource singleAudio;

        /// <summary>
        /// 音效列表
        /// </summary>
        private static List<AudioSource> soundAudios = new();

        /// <summary>
        /// 世界声音
        /// </summary>
        private Dictionary<GameObject, AudioSource> worldAudios = new();

        /// <summary>
        /// 正在播放的音乐容器
        /// </summary>
        private readonly Dictionary<int, AudioSource> playingAudioContainers = new();

        /// <summary>
        /// 已取消但还在等待加载的音频ID
        /// </summary>
        private readonly HashSet<int> canceledAudioHandleIds = new();

        /// <summary>
        /// 下一个音频播放ID
        /// </summary>
        private int nextAudioHandleId = 1;

        /// <summary>
        /// 声音列表
        /// </summary>
        private static Dictionary<string, AudioClip> allAudioClipsDic = new();

        /// <summary>
        /// 声音混合器
        /// </summary>
        private AudioMixer mixer;

        const string MusicVolumeTag = "BGMVolume";
        const string SoundVolumeTag = "SoundVolume";
        const string DialogueVolumeTag = "DialogueVolume";
        const string WorldVolumeTag = "WorldVolume";

        #endregion

        #region 声音管理

        private bool isMute; //是否静音
        private float bgmValue = 0.6f;
        private float singleValue = 1f;
        private float soundValue = 1f;
        private float worldValue = 1f;

        /// <summary>
        /// 静音
        /// </summary>
        public bool Mute
        {
            get => isMute;
            set
            {
                if (isMute == value) return;
                isMute = value;
                SetValue("Master", isMute ? 0 : 1);
            }
        }

        /// <summary>
        /// 背景音量
        /// </summary>
        public float BGMValue
        {
            get => bgmValue;
            set
            {
                if (Mathf.Approximately(bgmValue, value)) return;
                bgmValue = value;
                SetValue(MusicVolumeTag, bgmValue);
            }
        }

        /// <summary>
        /// 对话音量
        /// </summary>
        public float SingleValue
        {
            get => singleValue;
            set
            {
                if (Mathf.Approximately(singleValue, value)) return;
                singleValue = value;
                SetValue(DialogueVolumeTag, singleValue);
            }
        }

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SoundValue
        {
            get => soundValue;
            set
            {
                if (Mathf.Approximately(soundValue, value)) return;
                soundValue = value;
                SetValue(SoundVolumeTag, soundValue);
            }
        }

        /// <summary>
        /// 世界音量
        /// </summary>
        public float WorldVolume
        {
            get => worldValue;
            set
            {
                if (Mathf.Approximately(worldValue, value)) return;
                worldValue = value;
                SetValue(WorldVolumeTag, worldValue);
            }
        }

        /// <summary>
        /// 对话结束事件
        /// </summary>
        public event Action SingleSoundEndOfPlayEvent;

        /// <summary>
        /// 对话是否开始
        /// </summary>
        private bool isDialogueStart;

        private void SetValue(string name, float value)
        {
            if (mixer == null)
            {
                mixer = Resources.Load<AudioMixer>("Audios/AudioMixer");
            }

            if (mixer == null)
            {
                Debug.LogError($"声音混合器为:{mixer}");
            }

            if (value <= 0.0001f)
            {
                mixer?.SetFloat(name, -80f);
                return;
            }

            //mixer是按照-80分贝->20分贝的范围  去掉前30分贝--听不到&去掉后20分贝--增加音量
            //float volume = Mathf.Lerp(-50f, 0f,value);
            mixer?.SetFloat(name, Mathf.Log10(value) * 20f);
        }

        #endregion

        #region 生命周期

        private static YangAudioManager instance;

        public static YangAudioManager Instance => instance;

        /// <summary>
        /// 模块初始化
        /// </summary>
        public void Init()
        {
            mixer = Resources.Load<AudioMixer>("Audios/AudioMixer");
            managerObject = new GameObject("AudioManagerObject");
            managerObject.transform.SetParent(Core.YangToolsManager.DontDestoryObject.transform);

            bgmAudio = CreateAudioSource("BGMusic", BackgroundPriorityDefault, 0.6f, 1f, 0);
            bgmAudio.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
            singleAudio = CreateAudioSource("singleMusic", SinglePriorityDefault, 1f, 1f, 0);
            singleAudio.outputAudioMixerGroup = mixer.FindMatchingGroups("Dialogue")[0];
            for (int i = 0; i < 10; i++)
            {
                AudioSource temp = CreateAudioSource($"soundMusic_{i}", MultiplePriorityDefault, 1f, 1f, 0);
                temp.outputAudioMixerGroup = mixer.FindMatchingGroups("Sound")[0];
                soundAudios.Add(temp);
            }
        }

        public void Update()
        {
        }

        internal override void InitModule()
        {
            if (instance == null)
            {
                instance = this;
                Init();
            }
        }

        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            if (isDialogueStart)
            {
                if (!singleAudio.isPlaying)
                {
                    isDialogueStart = false;
                    RemoveAudioSourceHandles(singleAudio);
                    SingleSoundEndOfPlayEvent?.Invoke();
                }
            }
        }

        internal override void CloseModule()
        {
            StopBGM();
            StopSingleSound();
            StopAllMultipleSound();
            StopAllWorldSound();
        }

        #endregion

        #region 背景音乐

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="audioName">音乐名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        /// <returns>音频播放句柄</returns>
        public AudioHandle PlayBGM(string audioName, bool isLoop = true, float speed = 1f)
        {
            AudioHandle handle = CreateAudioHandle();
            PlayBGMAsync(handle.ID, audioName, isLoop, speed).Forget();
            return handle;
        }

        /// <summary>
        /// 异步播放背景音乐
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        /// <param name="audioName">音乐名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private async UniTask PlayBGMAsync(int audioId, string audioName, bool isLoop, float speed)
        {
            AudioClip clip = await GetAudioClip(audioName);
            if (clip == null || IsAudioHandleCanceled(audioId)) return;

            if (bgmAudio.isPlaying)
            {
                bgmAudio.Stop();
            }

            RemoveAudioSourceHandles(bgmAudio);
            playingAudioContainers[audioId] = bgmAudio;
            bgmAudio.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
            bgmAudio.clip = clip;
            bgmAudio.loop = isLoop;
            bgmAudio.pitch = speed;
            bgmAudio.Play();
        }

        /// <summary>
        /// 暂停播放背景音乐
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void PauseBGM(bool isGradual = true)
        {
            if (isGradual)
            {
                bgmAudio.DOFade(0, 2)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        bgmAudio.volume = BGMValue;
                        bgmAudio.Pause();
                    });
            }
            else
            {
                bgmAudio.Pause();
            }
        }

        /// <summary>
        /// 恢复播放背景音乐
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void UnPauseBGM(bool isGradual = true)
        {
            if (isGradual)
            {
                bgmAudio.UnPause();
                bgmAudio.volume = 0;
                bgmAudio.DOFade(BGMValue, 2).SetUpdate(true);
            }
            else
            {
                bgmAudio.UnPause();
            }
        }

        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopBGM()
        {
            if (bgmAudio && bgmAudio.isPlaying)
            {
                bgmAudio.Stop();
            }

            RemoveAudioSourceHandles(bgmAudio);
        }

        #endregion

        #region 对话音效

        /// <summary>
        /// 播放对话音效
        /// </summary>
        /// <param name="audioName">声音名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        /// <returns>音频播放句柄</returns>
        public AudioHandle PlaySingleSound(string audioName, bool isLoop = false, float speed = 1f)
        {
            AudioHandle handle = CreateAudioHandle();
            PlaySingleSoundAsync(handle.ID, audioName, isLoop, speed).Forget();
            return handle;
        }

        /// <summary>
        /// 异步播放对话音效
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        /// <param name="audioName">声音名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private async UniTask PlaySingleSoundAsync(int audioId, string audioName, bool isLoop, float speed)
        {
            AudioClip clip = await GetAudioClip(audioName);
            if (clip == null || IsAudioHandleCanceled(audioId)) return;
            PlaySingleSound(clip, isLoop, speed, audioId);
        }

        /// <summary>
        /// 播放对话音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        /// <param name="audioId">音频播放ID</param>
        private void PlaySingleSound(AudioClip clip, bool isLoop = false, float speed = 1f, int audioId = 0)
        {
            if (singleAudio.isPlaying)
            {
                singleAudio.Stop();
            }

            RemoveAudioSourceHandles(singleAudio);
            if (audioId > 0)
            {
                playingAudioContainers[audioId] = singleAudio;
            }
            singleAudio.outputAudioMixerGroup = mixer.FindMatchingGroups("Dialogue")[0];
            singleAudio.clip = clip;
            singleAudio.loop = isLoop;
            singleAudio.pitch = speed;
            singleAudio.Play();
            isDialogueStart = true;
        }

        /// <summary>
        /// 暂停播放单通道音效
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void PauseSingleSound(bool isGradual = true)
        {
            if (isGradual)
            {
                singleAudio.DOFade(0, 2)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        singleAudio.volume = SingleValue;
                        singleAudio.Pause();
                    });
            }
            else
            {
                singleAudio.Pause();
            }
        }

        /// <summary>
        /// 恢复播放单通道音效
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void UnPauseSingleSound(bool isGradual = true)
        {
            if (isGradual)
            {
                singleAudio.UnPause();
                singleAudio.volume = 0;
                singleAudio.DOFade(SingleValue, 2).SetUpdate(true);
            }
            else
            {
                singleAudio.UnPause();
            }
        }

        /// <summary>
        /// 停止播放单通道音效
        /// </summary>
        public void StopSingleSound()
        {
            if (singleAudio && singleAudio.isPlaying)
            {
                singleAudio.Stop();
            }

            RemoveAudioSourceHandles(singleAudio);
        }

        #endregion

        #region 音效

        /// <summary>
        /// 播放循环音效
        /// </summary>
        /// <param name="audioName">声音名字</param>
        /// <param name="speed">播放速度</param>
        /// <returns>音频播放句柄</returns>
        public AudioHandle PlayLoopSoundAudio(string audioName, float speed = 1)
        {
            return PlaySoundAudio(audioName, true, speed);
        }
        
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="audioName">声音名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        /// <returns>音频播放句柄</returns>
        public AudioHandle PlaySoundAudio(string audioName, bool isLoop = false, float speed = 1)
        {
            AudioHandle handle = CreateAudioHandle();
            PlaySoundAudioAsync(handle.ID, audioName, isLoop, speed).Forget();
            return handle;
        }

        /// <summary>
        /// 异步播放音效
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        /// <param name="audioName">声音名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private async UniTask PlaySoundAudioAsync(int audioId, string audioName, bool isLoop, float speed)
        {
            AudioClip clip = await GetAudioClip(audioName);
            if (clip == null || IsAudioHandleCanceled(audioId)) return;
            PlaySoundAudio(clip, isLoop, speed, audioId);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private void PlaySoundAudio(AudioClip clip, bool isLoop = false, float speed = 1, int audioId = 0)
        {
            AudioSource audio = ExtractIdleSoundAudioSource();
            if (audio)
            {
                if (mixer) audio.outputAudioMixerGroup = mixer.FindMatchingGroups("Sound")[0];
                audio.clip = clip;
                audio.loop = isLoop;
                audio.pitch = speed;
                if (audioId > 0)
                {
                    playingAudioContainers[audioId] = audio;
                }
                audio.Play();
            }
            else
            {
                Debug.LogWarning("音效音源不足，无法播放音效");
                if (audioId > 0)
                {
                    playingAudioContainers.Remove(audioId);
                }
            }
        }

        /// <summary>
        /// 停止播放所有音效
        /// </summary>
        public void StopAllMultipleSound()
        {
            for (int i = 0; i < soundAudios.Count; i++)
            {
                if (soundAudios[i] && soundAudios[i].isPlaying)
                {
                    soundAudios[i].Stop();
                    RemoveAudioSourceHandles(soundAudios[i]);
                }
            }
        }

        //提取闲置中的音效音源
        private AudioSource ExtractIdleSoundAudioSource()
        {
            for (int i = 0; i < soundAudios.Count; i++)
            {
                if (!soundAudios[i].isPlaying)
                {
                    RemoveAudioSourceHandles(soundAudios[i]);
                    return soundAudios[i];
                }
            }

            AudioSource audio = CreateAudioSource("SoundAudio", MultiplePriorityDefault, SoundValue, 1, 0);
            soundAudios.Add(audio);
            return audio;
        }

        /// <summary>
        /// 销毁所有闲置中的多通道音效的音源
        /// </summary>
        public void ClearIdleSoundAudioSource()
        {
            for (int i = 0; i < soundAudios.Count; i++)
            {
                if (!soundAudios[i].isPlaying)
                {
                    AudioSource audio = soundAudios[i];
                    RemoveAudioSourceHandles(audio);
                    soundAudios.RemoveAt(i);
                    i -= 1;
                    GameObject.Destroy(audio.gameObject);
                }
            }
        }

        #endregion

        #region 世界音效

        /// <summary>
        /// 播放世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="audioName">音乐名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        /// <returns>音频播放句柄</returns>
        public AudioHandle PlayWorldSound(GameObject attachTarget, string audioName, bool isLoop = false,
            float speed = 1)
        {
            AudioHandle handle = CreateAudioHandle();
            PlayWorldSoundAsync(handle.ID, attachTarget, audioName, isLoop, speed).Forget();
            return handle;
        }

        /// <summary>
        /// 异步播放世界音效
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="audioName">音乐名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private async UniTask PlayWorldSoundAsync(int audioId, GameObject attachTarget, string audioName,
            bool isLoop, float speed)
        {
            AudioClip clip = await GetAudioClip(audioName);
            if (clip == null || IsAudioHandleCanceled(audioId)) return;
            PlayWorldSound(attachTarget, clip, isLoop, speed, audioId);
        }

        /// <summary>
        /// 播放世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        /// <param name="audioId">音频播放ID</param>
        private void PlayWorldSound(GameObject attachTarget, AudioClip clip, bool isLoop = false, float speed = 1,
            int audioId = 0)
        {
            if (attachTarget == null)
            {
                canceledAudioHandleIds.Remove(audioId);
                playingAudioContainers.Remove(audioId);
                Debug.LogWarning("世界音效挂载目标为空，无法播放音效");
                return;
            }

            if (worldAudios.TryGetValue(attachTarget, out var worldAudio))
            {
                if (worldAudio.isPlaying)
                {
                    worldAudio.Stop();
                }

                RemoveAudioSourceHandles(worldAudio);
                if (audioId > 0)
                {
                    playingAudioContainers[audioId] = worldAudio;
                }
                worldAudio.outputAudioMixerGroup = mixer.FindMatchingGroups("World")[0];
                worldAudio.clip = clip;
                worldAudio.loop = isLoop;
                worldAudio.pitch = speed;
                worldAudio.Play();
            }
            else
            {
                AudioSource audio = AttachAudioSource(attachTarget, WorldPriorityDefault, WorldVolume, 1, 1);
                audio.outputAudioMixerGroup = mixer.FindMatchingGroups("World")[0];
                worldAudios.Add(attachTarget, audio);
                if (audioId > 0)
                {
                    playingAudioContainers[audioId] = audio;
                }
                audio.clip = clip;
                audio.loop = isLoop;
                audio.pitch = speed;
                audio.Play();
            }
        }

        /// <summary>
        /// 暂停播放指定的世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="isGradual">是否渐进式</param>
        public void PauseWorldSound(GameObject attachTarget, bool isGradual = true)
        {
            if (worldAudios.TryGetValue(attachTarget, out var audio))
            {
                if (isGradual)
                {
                    audio.DOFade(0, 2)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            audio.volume = WorldVolume;
                            audio.Pause();
                        });
                }
                else
                {
                    audio.Pause();
                }
            }
        }

        /// <summary>
        /// 恢复播放指定的世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="isGradual">是否渐进式</param>
        public void UnPauseWorldSound(GameObject attachTarget, bool isGradual = true)
        {
            if (worldAudios.TryGetValue(attachTarget, out var audio))
            {
                if (isGradual)
                {
                    audio.UnPause();
                    audio.volume = 0;
                    audio.DOFade(WorldVolume, 2).SetUpdate(true);
                }
                else
                {
                    audio.UnPause();
                }
            }
        }

        /// <summary>
        /// 停止播放所有的世界音效
        /// </summary>
        public void StopAllWorldSound()
        {
            foreach (var audio in worldAudios)
            {
                if (audio.Value.isPlaying)
                {
                    audio.Value.Stop();
                    RemoveAudioSourceHandles(audio.Value);
                }
            }
        }

        /// <summary>
        /// 销毁所有闲置中的世界音效的音源
        /// </summary>
        public void ClearIdleWorldAudioSource()
        {
            HashSet<GameObject> removeSet = new HashSet<GameObject>();
            foreach (var audio in worldAudios)
            {
                if (!audio.Value.isPlaying)
                {
                    removeSet.Add(audio.Key);
                    RemoveAudioSourceHandles(audio.Value);
                    Object.Destroy(audio.Value);
                }
            }

            foreach (var item in removeSet)
            {
                worldAudios.Remove(item);
            }
        }

        //附加一个音源
        private AudioSource AttachAudioSource(GameObject target, int priority, float volume, float speed,
            float spatialBlend)
        {
            AudioSource audio = target.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.priority = priority;
            audio.volume = volume;
            audio.pitch = speed;
            audio.spatialBlend = spatialBlend;
            audio.mute = isMute;
            return audio;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 创建音频播放句柄
        /// </summary>
        /// <returns>音频播放句柄</returns>
        private AudioHandle CreateAudioHandle()
        {
            int id = nextAudioHandleId++;
            canceledAudioHandleIds.Remove(id);
            return new AudioHandle(id, this);
        }

        /// <summary>
        /// 停止指定ID的音频
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        public void StopAudio(int audioId)
        {
            if (!playingAudioContainers.TryGetValue(audioId, out var audio))
            {
                canceledAudioHandleIds.Add(audioId);
                return;
            }

            if (audio)
            {
                audio.Stop();
            }

            playingAudioContainers.Remove(audioId);
            canceledAudioHandleIds.Remove(audioId);
        }

        /// <summary>
        /// 暂停指定ID的音频
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        public void PauseAudio(int audioId)
        {
            if (playingAudioContainers.TryGetValue(audioId, out var audio) && audio)
            {
                audio.Pause();
            }
        }

        /// <summary>
        /// 恢复指定ID的音频
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        public void ResumeAudio(int audioId)
        {
            if (playingAudioContainers.TryGetValue(audioId, out var audio) && audio)
            {
                audio.UnPause();
            }
        }

        /// <summary>
        /// 判断音频句柄是否已经取消
        /// </summary>
        /// <param name="audioId">音频播放ID</param>
        /// <returns>是否已经取消</returns>
        private bool IsAudioHandleCanceled(int audioId)
        {
            if (!canceledAudioHandleIds.Remove(audioId)) return false;

            playingAudioContainers.Remove(audioId);
            return true;
        }

        /// <summary>
        /// 移除指定音源关联的播放句柄
        /// </summary>
        /// <param name="audioSource">音源</param>
        private void RemoveAudioSourceHandles(AudioSource audioSource)
        {
            List<int> removeIds = new List<int>();
            foreach (var item in playingAudioContainers)
            {
                if (item.Value == audioSource)
                {
                    removeIds.Add(item.Key);
                }
            }

            for (int i = 0; i < removeIds.Count; i++)
            {
                playingAudioContainers.Remove(removeIds[i]);
            }
        }

        /// <summary>
        /// 创建一个音源
        /// </summary>
        /// <param name="name">物体名称</param>
        /// <param name="priority">声音优先度</param>
        /// <param name="volume">声音大小</param>
        /// <param name="speed">声音速度</param>
        /// <param name="spatialBlend">混合 0:2d声音 1:3d声音</param>
        /// <returns></returns>
        private static AudioSource CreateAudioSource(string name, int priority, float volume, float speed,
            float spatialBlend)
        {
            GameObject audioObj = new GameObject(name);
            audioObj.transform.SetParent(managerObject.transform);
            audioObj.transform.localPosition = Vector3.zero;
            audioObj.transform.localRotation = Quaternion.identity;
            audioObj.transform.localScale = Vector3.one;
            AudioSource audio = audioObj.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.priority = priority;
            audio.volume = volume;
            audio.pitch = speed;
            audio.spatialBlend = spatialBlend;
            audio.mute = false;
            return audio;
        }

        /// <summary>
        /// 加载中的声音
        /// </summary>
        private static readonly List<string> LoadingAudioNames = new List<string>();

        /// <summary>
        /// 获取声音
        /// </summary>
        private static async UniTask<AudioClip> GetAudioClip(string audioName)
        {
            if (allAudioClipsDic.TryGetValue(audioName, out var clip))
            {
                return clip;
            }

            while (LoadingAudioNames.Contains(audioName))
            {
                // 等待同名音频加载完成后，优先复用已写入的缓存，避免重复加载。
                await UniTask.WaitUntil(() => !LoadingAudioNames.Contains(audioName));
                if (allAudioClipsDic.TryGetValue(audioName, out clip))
                {
                    return clip;
                }
            }

            LoadingAudioNames.Add(audioName);
            try
            {
                AudioClip target = await LoadAudioClip(audioName);
                if (target != null)
                {
                    allAudioClipsDic[audioName] = target;
                    return target;
                }
            }
            finally
            {
                LoadingAudioNames.Remove(audioName);
            }

            Debug.LogError("找不到声音资源：" + audioName);
            return null;
        }

        /// <summary>
        /// 加载声音
        /// </summary>
        private static async UniTask<AudioClip> LoadAudioClip(string audioName)
        {
            var audioClip = await ResourceManager.ResourceManager.LoadAudioClip(audioName);
            return audioClip;
        }

        #endregion
    }
}
