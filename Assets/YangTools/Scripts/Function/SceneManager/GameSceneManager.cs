﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YangTools
{
    /// <summary>
    /// 场景加载回调
    /// </summary>
    public class LoadOnProgress
    {
        /// <summary>
        /// 加载开始回调
        /// </summary>
        public Action<string> OnStartLoad;
        /// <summary>
        /// 加载中回调
        /// </summary>
        public Action<float> OnLoading;
        /// <summary>
        /// 加载结束回调
        /// </summary>
        public Action<string> OnEndLoad;
    }

    /// <summary>
    /// 场景管理器
    /// </summary>
    public class GameSceneManager : MonoSingleton<GameSceneManager>
    {
        #region 变量
        private string sceneName;
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName => sceneName;
        private LoadSceneMode sceneLoadType;
        /// <summary>
        /// 场景加载类型
        /// </summary>
        public LoadSceneMode SceneLoadType => sceneLoadType;
        private bool isAutoSkip;//自动跳转场景
        /// <summary>
        /// 加载完后自动跳转
        /// </summary>
        public bool IsAutoSkip
        {
            get
            {
                return isAutoSkip;
            }

            set
            {
                isAutoSkip = value;
                if (async != null)
                {
                    async.allowSceneActivation = isAutoSkip;
                }
            }
        }
        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get { return async != null; }
        }
        private float progress = 0;
        /// <summary>
        /// 加载进度
        /// </summary>
        public float Progress => progress;
        //异步加载信息
        private AsyncOperation async;
        private LoadOnProgress onProgressEvent;
        /// <summary>
        /// 加载事件
        /// </summary>
        public LoadOnProgress OnProgressEvent => onProgressEvent;

        #endregion

        #region 生命周期
        /// <summary>
        /// 更新加载进度
        /// </summary>
        private void Update()
        {
            if (async == null)
            {
                enabled = false;
            }
            else
            {
                // 加载进度值范围是0~0.9 这里做下处理
                float progress = async.progress;
                progress = progress < 0.9f ? progress * (10f / 9f) : 1f;
                this.progress = progress;
                OnProgressEvent?.OnLoading?.Invoke(progress);
            }
        }
        #endregion

        #region 对内方法
        /// <summary>
        /// 加载场景
        /// </summary>
        private void LoadScene()
        {
            if (IsLoading) return;
            progress = 0;
            enabled = true;
            OnProgressEvent?.OnStartLoad?.Invoke(sceneName);
            StartCoroutine(Loading());
        }
        /// <summary>
        /// 开启协程加载场景
        /// </summary>
        private IEnumerator Loading()
        {
            async = SceneManager.LoadSceneAsync(sceneName, sceneLoadType);
            // 设置加载完成后不能自动跳转场景
            async.allowSceneActivation = isAutoSkip;
            yield return async;
            OnProgressEvent?.OnEndLoad?.Invoke(sceneName);
            async = null;
        }

#if UNITY_EDITOR
        //屏蔽Animation关键帧事件中的loading()可选项---会提示警告-并且事件函数会失效
        private IEnumerator Loading(string sceneName)
        {
            yield return null;
        }
#endif

        #endregion

        #region 对外方法
        /// <summary>
        /// 加载场景
        /// </summary>
        public void Load(string _sceneName, LoadSceneMode _sceneLoadType = LoadSceneMode.Single)
        {
            sceneName = _sceneName;
            sceneLoadType = _sceneLoadType;

            LoadScene();
        }
        /// <summary>
        /// 设置加载事件
        /// </summary>
        public void SetOnProgress(LoadOnProgress onProgress)
        {
            onProgressEvent = onProgress;
        }
        #endregion
    }
}