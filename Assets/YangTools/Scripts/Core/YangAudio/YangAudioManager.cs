using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using YangTools;

namespace YangTools
{
    /// <summary>
    /// 声音管理者
    /// </summary>
    internal class YangAudioManager : GameModuleManager
    {
        public static YangAudioManager Instance { get; private set; }

        #region 属性
        //设备一般最多允许32个音源同时播放,优先级参数决定了在超出音源数目时，需要暂时关闭一些不重要的音源，优先播放更重要的音源
        /// <summary>
        /// 背景音乐优先级
        /// </summary>
        private static readonly int BackgroundPriorityDefault = 0;
        /// <summary>
        /// 单通道音效优先级
        /// </summary>
        private static readonly int SinglePriorityDefault = 10;
        /// <summary>
        /// 多通道音效优先级
        /// </summary>
        private static readonly int MultiplePriorityDefault = 20;
        /// <summary>
        /// 世界音效优先级
        /// </summary>
        private static readonly int WorldPriorityDefault = 30;
        /// <summary>
        /// 声音的物体
        /// </summary>
        public static GameObject audioObject;
        /// <summary>
        /// 背景
        /// </summary>
        private static AudioSource backgroundAudio;
        /// <summary>
        /// 对话--单通道
        /// </summary>
        private static AudioSource singleAudio;
        /// <summary>1
        /// 音效列表
        /// </summary>
        private static List<AudioSource> soundAudios = new List<AudioSource>();
        /// <summary>
        /// 世界声音
        /// </summary>
        private Dictionary<GameObject, AudioSource> worldAudios = new Dictionary<GameObject, AudioSource>();
        /// <summary>
        /// 声音列表
        /// </summary>
        public static Dictionary<string, AudioClip> audioClipDictionary = new Dictionary<string, AudioClip>();
        #endregion

        #region 声音管理
        private bool _isMute = false;//是否静音
        /// <summary>
        /// 静音
        /// </summary>
        public bool Mute
        {
            get
            {
                return _isMute;
            }
            set
            {
                if (_isMute != value)
                {
                    _isMute = value;
                    backgroundAudio.mute = _isMute;
                    singleAudio.mute = _isMute;
                    for (int i = 0; i < soundAudios.Count; i++)
                    {
                        soundAudios[i].mute = _isMute;
                    }
                    foreach (var audio in worldAudios)
                    {
                        audio.Value.mute = _isMute;
                    }
                }
            }
        }

        private float _backgroundVolume = 0.6f;//背景音乐声音大小
        private float _singleVolume = 1f;//声音大小
        private float _soundVolume = 1f;//声音大小
        private float _worldVolume = 1f;//声音大小

        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float BackgroundVolume
        {
            get
            {
                return _backgroundVolume;
            }
            set
            {
                if (!Mathf.Approximately(_backgroundVolume, value))
                {
                    _backgroundVolume = value;
                    backgroundAudio.volume = _backgroundVolume;
                }
            }
        }
        /// <summary>
        /// 对话-单通道音效音量
        /// </summary>
        public float SingleVolume
        {
            get
            {
                return _singleVolume;
            }
            set
            {
                if (!Mathf.Approximately(_singleVolume, value))
                {
                    _singleVolume = value;
                    singleAudio.volume = _singleVolume;
                }
            }
        }
        /// <summary>
        /// 音效音量
        /// </summary>
        public float SoundVolume
        {
            get
            {
                return _soundVolume;
            }
            set
            {

                if (!Mathf.Approximately(_soundVolume, value))
                {
                    _soundVolume = value;
                    for (int i = 0; i < soundAudios.Count; i++)
                    {
                        soundAudios[i].volume = _soundVolume;
                    }
                }
            }
        }
        /// <summary>
        /// 世界音效音量
        /// </summary>
        public float WorldVolume
        {
            get
            {
                return _worldVolume;
            }
            set
            {
                if (!Mathf.Approximately(_worldVolume, value))
                {
                    _worldVolume = value;
                    foreach (var audio in worldAudios)
                    {
                        audio.Value.volume = _worldVolume;
                    }
                }
            }
        }
        /// <summary>
        /// 单通道音效播放结束事件
        /// </summary>
        public event Action singleSoundEndOfPlayEvent;
        private bool _singleSoundPlayDetector = false;//检查是否开启
        #endregion

        #region 初始化
        /// <summary>
        /// 构造函数，为保证顺序必须有
        /// </summary>
        static YangAudioManager()
        {

        }
        /// <summary>
        /// 初始化，会生成一个物体挂载在工具类不可删除物体下
        /// </summary>
        internal override void Init()
        {
            audioObject = new GameObject("AudioManagerObject");
            audioObject.transform.SetParent(YangToolsManager.DontDestoryObject.transform);

            backgroundAudio = CreateAudioSource("BGMusic", BackgroundPriorityDefault, 0.6f, 1f, 0);
            singleAudio = CreateAudioSource("singleMusic", SinglePriorityDefault, 1f, 1f, 0);
            for (int i = 0; i < 10; i++)
            {
                AudioSource temp = CreateAudioSource($"soundMusic_{i}", MultiplePriorityDefault, 1f, 1f, 0);
                soundAudios.Add(temp);
            }

            LoadAllAudio();
        }
        /// <summary>
        /// 加载Resources里对应位置的Aduio
        /// </summary>
        public static void LoadAllAudio()
        {
            audioClipDictionary.Clear();
            //本地加载 
            AudioClip[] BGAudioArray = Resources.LoadAll<AudioClip>("/BGMusic");
            AudioClip[] SoundAudioArray = Resources.LoadAll<AudioClip>("/SoundMusic");

            IEnumerable<AudioClip> tempArray = BGAudioArray.Concat(SoundAudioArray);

            //存放到字典
            foreach (AudioClip item in tempArray)
            {
                audioClipDictionary.Add(item.name, item);
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
        private static AudioSource CreateAudioSource(string name, int priority, float volume, float speed, float spatialBlend)
        {
            GameObject audioObj = new GameObject(name);
            audioObj.transform.SetParent(audioObject.transform);
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
        #endregion

        #region 生命周期
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_singleSoundPlayDetector)
            {
                if (!singleAudio.isPlaying)
                {
                    _singleSoundPlayDetector = false;
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
        //==================背景音乐=====================
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlayBackgroundMusic(string audioName, bool isLoop = true, float speed = 1f)
        {
            AudioClip clip = GetClipFromDic(audioName);
            if (clip == null) return;

            if (backgroundAudio.isPlaying)
            {
                backgroundAudio.Stop();
            }
            backgroundAudio.clip = clip;
            backgroundAudio.loop = isLoop;
            backgroundAudio.pitch = speed;
            backgroundAudio.Play();
        }

        /// <summary>
        /// 暂停播放背景音乐
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void PauseBackgroundMusic(bool isGradual = true)
        {
            if (isGradual)
            {
                backgroundAudio.DOFade(0, 2)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    backgroundAudio.volume = BackgroundVolume;
                    backgroundAudio.Pause();
                });
            }
            else
            {
                backgroundAudio.Pause();
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
                backgroundAudio.UnPause();
                backgroundAudio.volume = 0;
                backgroundAudio.DOFade(BackgroundVolume, 2).SetUpdate(true);
            }
            else
            {
                backgroundAudio.UnPause();
            }
        }

        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (backgroundAudio.isPlaying)
            {
                backgroundAudio.Stop();
            }
        }

        //==================背景音乐END=====================
        #endregion

        #region 对话音效
        //==================对话音效=====================
        /// <summary>
        /// 播放对话--单通道音效
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
        /// 播放对话--单通道音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlaySingleSound(AudioClip clip, bool isLoop = false, float speed = 1f)
        {
            if (singleAudio.isPlaying)
            {
                singleAudio.Stop();
            }

            singleAudio.clip = clip;
            singleAudio.loop = isLoop;
            singleAudio.pitch = speed;
            singleAudio.Play();
            _singleSoundPlayDetector = true;
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
        //==================对话END=====================
        #endregion

        #region 音效
        //==================音效=====================
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
        public void PlaySoundAudio(AudioClip clip, bool isLoop = false, float speed = 1)
        {
            AudioSource audio = ExtractIdleSoundAudioSource();
            audio.clip = clip;
            audio.loop = isLoop;
            audio.pitch = speed;
            audio.Play();
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
        public void StopSoundAudio(AudioClip clip)
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
        //==================音效END=====================
        #endregion

        #region 世界音效
        //==================世界音效=====================
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
        public void PlayWorldSound(GameObject attachTarget, AudioClip clip, bool isLoop = false, float speed = 1)
        {
            if (worldAudios.ContainsKey(attachTarget))
            {
                AudioSource audio = worldAudios[attachTarget];
                if (audio.isPlaying)
                {
                    audio.Stop();
                }
                audio.clip = clip;
                audio.loop = isLoop;
                audio.pitch = speed;
                audio.Play();
            }
            else
            {
                AudioSource audio = AttachAudioSource(attachTarget, WorldPriorityDefault, WorldVolume, 1, 1);
                worldAudios.Add(attachTarget, audio);
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
            if (worldAudios.ContainsKey(attachTarget))
            {
                AudioSource audio = worldAudios[attachTarget];
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
            if (worldAudios.ContainsKey(attachTarget))
            {
                AudioSource audio = worldAudios[attachTarget];
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
                    GameObject.Destroy(audio.Value);
                }
            }
            foreach (var item in removeSet)
            {
                worldAudios.Remove(item);
            }
        }

        //附加一个音源
        private AudioSource AttachAudioSource(GameObject target, int priority, float volume, float speed, float spatialBlend)
        {
            AudioSource audio = target.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.priority = priority;
            audio.volume = volume;
            audio.pitch = speed;
            audio.spatialBlend = spatialBlend;
            audio.mute = _isMute;
            return audio;
        }
        //=================世界音效END====================
        #endregion

        #region 获得声音Clip
        /// <summary>
        /// 从字典里获取声音
        /// </summary>
        /// <returns></returns>
        public static AudioClip GetClipFromDic(string audioName)
        {
            if (audioClipDictionary.ContainsKey(audioName))
            {
                return audioClipDictionary[audioName];
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