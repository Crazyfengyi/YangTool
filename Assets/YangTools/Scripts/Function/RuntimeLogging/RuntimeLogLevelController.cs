using UnityEngine;

namespace XYBTMiniGame
{
    /// <summary>
    /// Unity运行时日志输出等级
    /// </summary>
    public enum RuntimeLogLevel
    {
        [InspectorName("全部")]
        All,

        [InspectorName("警告及以上")]
        Warning,

        [InspectorName("仅错误")]
        Error,

        [InspectorName("关闭")]
        None
    }

    /// <summary>
    /// Unity运行时全局日志等级控制器
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    [AddComponentMenu("XYBT SDK/Runtime Log Level Controller")]
    public sealed class RuntimeLogLevelController : MonoBehaviour
    {
        // 首个初始化的控制器实例
        private static RuntimeLogLevelController instance;

        // Inspector中配置的运行时日志等级
        [SerializeField]
        [InspectorName("运行时日志等级")]
        private RuntimeLogLevel logLevel = RuntimeLogLevel.All;

        /// <summary>
        /// 初始化全局日志配置并保持跨场景生效
        /// </summary>
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyLogLevel();
        }

        /// <summary>
        /// 清理已销毁的控制器实例
        /// </summary>
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 根据Inspector配置设置Unity全局日志筛选条件
        /// </summary>
        private void ApplyLogLevel()
        {
            switch (logLevel)
            {
                case RuntimeLogLevel.All:
                    Debug.unityLogger.logEnabled = true;
                    Debug.unityLogger.filterLogType = LogType.Log;
                    break;

                case RuntimeLogLevel.Warning:
                    Debug.unityLogger.logEnabled = true;
                    Debug.unityLogger.filterLogType = LogType.Warning;
                    break;

                case RuntimeLogLevel.Error:
                    Debug.unityLogger.logEnabled = true;
                    Debug.unityLogger.filterLogType = LogType.Error;
                    break;

                case RuntimeLogLevel.None:
                    Debug.unityLogger.logEnabled = false;
                    break;
            }
        }
    }
}
