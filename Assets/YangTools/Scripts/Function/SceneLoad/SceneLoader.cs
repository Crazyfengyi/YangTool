using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace YangTools
{
    /// <summary>
    /// 进度更新接口
    /// </summary>
    public interface IOnProgress
    {
        /// <summary>
        /// 获得进度条--会控制显隐
        /// </summary>
        /// <returns>GameObject</returns>
        GameObject GetprogressReceiver();
        /// <summary>
        /// 进度更新事件
        /// </summary>
        /// <param name="progress">进度0~1</param>
        void onProgress(float progress);
    }
    /// <summary>
    /// 场景加载器
    /// </summary>
    public class SceneLoader : MonoSingleton<SceneLoader>
    {
        #region 变量
        /// <summary>
        /// 场景名称
        /// </summary>
        public string sceneName;
        /// <summary>
        /// 场景加载类型
        /// </summary>
        public LoadSceneMode sceneLoadType;
        /// <summary>
        /// 进度接收者
        /// </summary>
        public IOnProgress progressReceiver;
        /// <summary>
        /// 自动跳转场景
        /// </summary>
        private bool isAutoSkip;
        /// <summary>
        /// 加载完后是否--自动跳转场景
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
        /// 加载进度
        /// </summary>
        private float _progress = 0;
        /// <summary>
        /// 异步加载信息
        /// </summary>
        private AsyncOperation async;
        /// <summary>
        /// 场景加载前
        /// </summary>
        public Action<string> OnSceneLoadPre;
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
                _progress = progress;
                progressReceiver?.onProgress(progress);
            }
        }
        #endregion

        #region 对内方法
        /// <summary>
        /// 加载场景
        /// </summary>
        private void loadScene()
        {
            if (isLoading) return;
            _progress = 0;
            enabled = true;
            StartCoroutine(Loading());
            progressReceiver?.GetprogressReceiver()?.SetActive(true);
            progressReceiver?.onProgress(progress);
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
            progressReceiver?.GetprogressReceiver()?.SetActive(false);
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

            OnSceneLoadPre?.Invoke(sceneName);
            loadScene();
        }
        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool isLoading
        {
            get { return async != null; }
        }
        /// <summary>
        /// 加载进度
        /// </summary>
        public float progress
        {
            get { return _progress; }
        }
        #endregion
    }
}