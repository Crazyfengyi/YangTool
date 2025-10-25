using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using YangTools.Scripts.Core;
using Object = UnityEngine.Object;

namespace YangTools.Scripts.Core.YangAudio
{
    /// <summary>
    /// 声音管理器
    /// </summary>
    public class YangAudioManager : GameModuleBase
    {
        private static YangAudioManager instance;

        public static YangAudioManager Instance => instance;

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
        /// 声音列表
        /// </summary>
        private static Dictionary<string, AudioClip> allAudioClipsDic = new();
        
        /// <summary>
        /// 声音混合器
        /// </summary>
        private AudioMixer mixer;
        #endregion

        #region 声音管理

        private bool isMute;//是否静音
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
                SetValue("BGM", bgmValue);
            }
        }

        /// <summary>
        /// 对话音量
        /// </summary>
        public float SingleVolume
        {
            get => singleValue;
            set
            {
                if (Mathf.Approximately(singleValue, value)) return;
                singleValue = value;
                SetValue("Dialogue", singleValue);
            }
        }

        /// <summary>
        /// 音效音量
        /// </summary>
        public float SoundVolume
        {
            get => soundValue;
            set
            {
                if (Mathf.Approximately(soundValue, value)) return;
                soundValue = value;
                SetValue("Sound", soundValue);
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
                SetValue("World", worldValue);
            }
        }

        /// <summary>
        /// 对话结束事件
        /// </summary>
        public event Action singleSoundEndOfPlayEvent;

        private bool singleSoundPlayDetector; //检查是否开启

        private void SetValue(string name, float value)
        {
            //mixer是按照-80分贝->20分贝的范围  去掉前30分贝--听不到&去掉后20分贝--增加音量
            float volume = Mathf.Lerp(-50f, 0f,value);
            mixer.SetFloat(name, volume);
        }
        #endregion

        #region 生命周期
        /// <summary>
        /// 构造函数,为保证顺序必须有
        /// </summary>
        static YangAudioManager()
        {
        }

        /// <summary>
        /// 模块初始化
        /// </summary>
        internal override void InitModule()
        {
            instance = new YangAudioManager();
            mixer = Resources.Load<AudioMixer>("Audios/AudioMixer");
            managerObject = new GameObject("AudioManagerObject");
            managerObject.transform.SetParent(Core.YangToolsManager.DontDestoryObject.transform);

            bgmAudio = CreateAudioSource("BGMusic", BackgroundPriorityDefault, 0.6f, 1f, 0);
            singleAudio = CreateAudioSource("singleMusic", SinglePriorityDefault, 1f, 1f, 0);
            for (int i = 0; i < 10; i++)
            {
                AudioSource temp = CreateAudioSource($"soundMusic_{i}", MultiplePriorityDefault, 1f, 1f, 0);
                soundAudios.Add(temp);
            }

            LoadAllAudio();
        }

        /// <summary>
        /// 加载Resources里对应位置的Audio
        /// </summary>
        public static void LoadAllAudio()
        {
            allAudioClipsDic.Clear();
            //本地加载 
            AudioClip[] BGAudioArray = Resources.LoadAll<AudioClip>("AudioBG");
            AudioClip[] SoundAudioArray = Resources.LoadAll<AudioClip>("AudioSound");

            IEnumerable<AudioClip> tempArray = BGAudioArray.Concat(SoundAudioArray);

            //存放到字典
            foreach (AudioClip item in tempArray)
            {
                allAudioClipsDic.Add(item.name, item);
            }
        }

        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            if (singleSoundPlayDetector)
            {
                if (!singleAudio.isPlaying)
                {
                    singleSoundPlayDetector = false;
                    singleSoundEndOfPlayEvent?.Invoke();
                }
            }
        }

        internal override void CloseModule()
        {
            StopBackgroundMusic();
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
        public void PlayBackgroundMusic(string audioName, bool isLoop = true, float speed = 1f)
        {
            AudioClip clip = GetClipFromDic(audioName);
            if (clip == null) return;

            if (bgmAudio.isPlaying)
            {
                bgmAudio.Stop();
            }

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
        public void PauseBackgroundMusic(bool isGradual = true)
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
        public void UnPauseBackgroundMusic(bool isGradual = true)
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
        public void StopBackgroundMusic()
        {
            if (bgmAudio.isPlaying)
            {
                bgmAudio.Stop();
            }
        }
        #endregion

        #region 对话音效

        /// <summary>
        /// 播放对话音效
        /// </summary>
        /// <param name="audioName">声音名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlaySingleSound(string audioName, bool isLoop = false, float speed = 1f)
        {
            AudioClip clip = GetClipFromDic(audioName);
            if (clip == null) return;
            PlaySingleSound(clip, isLoop, speed);
        }

        /// <summary>
        /// 播放对话音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private void PlaySingleSound(AudioClip clip, bool isLoop = false, float speed = 1f)
        {
            if (singleAudio.isPlaying)
            {
                singleAudio.Stop();
            }
            singleAudio.outputAudioMixerGroup = mixer.FindMatchingGroups("Dialogue")[0];
            singleAudio.clip = clip;
            singleAudio.loop = isLoop;
            singleAudio.pitch = speed;
            singleAudio.Play();
            singleSoundPlayDetector = true;
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
                        singleAudio.volume = SingleVolume;
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
                singleAudio.DOFade(SingleVolume, 2).SetUpdate(true);
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
            if (singleAudio.isPlaying)
            {
                singleAudio.Stop();
            }
        }

        #endregion

        #region 音效

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="audioName">声音名字</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlaySoundAudio(string audioName, bool isLoop = false, float speed = 1)
        {
            AudioClip clip = GetClipFromDic(audioName);
            if (clip == null) return;
            PlaySoundAudio(clip, isLoop, speed);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private void PlaySoundAudio(AudioClip clip, bool isLoop = false, float speed = 1)
        {
            AudioSource audio = ExtractIdleSoundAudioSource();
            if (audio)
            {
                if(mixer)audio.outputAudioMixerGroup = mixer.FindMatchingGroups("Sound")[0];
                audio.clip = clip;
                audio.loop = isLoop;
                audio.pitch = speed;
                audio.Play();  
            }
            else
            {
                Debug.LogWarning("音效音源不足，无法播放音效");
            }
        }

        /// <summary>
        /// 停止播放指定的音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        public void StopSoundAudio(string audioName)
        {
            AudioClip clip = GetClipFromDic(audioName);
            if (clip == null) return;
            StopSoundAudio(clip);
        }

        /// <summary>
        /// 停止播放指定的音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        private void StopSoundAudio(AudioClip clip)
        {
            for (int i = 0; i < soundAudios.Count; i++)
            {
                if (soundAudios[i].isPlaying)
                {
                    if (soundAudios[i].clip == clip)
                    {
                        soundAudios[i].Stop();
                    }
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
                if (soundAudios[i].isPlaying)
                {
                    soundAudios[i].Stop();
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
                    return soundAudios[i];
                }
            }

            AudioSource audio = CreateAudioSource("SoundAudio", MultiplePriorityDefault, SoundVolume, 1, 0);
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
        public void PlayWorldSound(GameObject attachTarget, string audioName, bool isLoop = false, float speed = 1)
        {
            AudioClip clip = GetClipFromDic(audioName);
            if (clip == null) return;
            PlayWorldSound(attachTarget, clip, isLoop, speed);
        }

        /// <summary>
        /// 播放世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        private void PlayWorldSound(GameObject attachTarget, AudioClip clip, bool isLoop = false, float speed = 1)
        {
            if (worldAudios.TryGetValue(attachTarget, out var worldAudio))
            {
                if (worldAudio.isPlaying)
                {
                    worldAudio.Stop();
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
                worldAudios.Add(attachTarget, audio);
                audio.outputAudioMixerGroup = mixer.FindMatchingGroups("World")[0];
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
        /// 从字典里获取声音
        /// </summary>
        public static AudioClip GetClipFromDic(string audioName)
        {
            if (allAudioClipsDic.TryGetValue(audioName, out var dic))
            {
                return dic;
            }
            else
            {
                Debug.LogError($"没有声音：{audioName}");
                return null;
            }
        }
        #endregion
    }
}