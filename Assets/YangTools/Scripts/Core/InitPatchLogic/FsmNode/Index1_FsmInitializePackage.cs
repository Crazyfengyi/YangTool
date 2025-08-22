using System.IO;
using System.Collections;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

namespace GameMain
{
    /// <summary>
    /// 初始化资源包
    /// </summary>
    internal class FsmInitializePackage : IStateNode
    {
        private StateMachine machine;

        public FsmInitializePackage()
        {
        }

        void IStateNode.OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        void IStateNode.OnEnter()
        {
            PatchStepsChange temp = new PatchStepsChange
            {
                Tips = "初始化资源包！"
            };
            temp.SendEvent();
            GameInit.Instance.StartCoroutine(InitPackage());
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private IEnumerator InitPackage()
        {
            var playMode = (EPlayMode)machine.GetBlackboardValue("PlayMode");
            var packageName = (string)machine.GetBlackboardValue("PackageName");
            var buildPipeline = (string)machine.GetBlackboardValue("BuildPipeline");

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
            {
                package = YooAssets.CreatePackage(packageName);
            }

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters =
                        FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot)
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters()
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(),
                    CacheFileSystemParameters =
                        FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices)
                };
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = package.InitializeAsync(createParameters);
#else
                var createParameters = new WebPlayModeParameters
                {
                    WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters()
                };
                initializationOperation = package.InitializeAsync(createParameters);
#endif
            }

            yield return initializationOperation;

            // 如果初始化失败弹出提示界面
            if (initializationOperation?.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning($"{initializationOperation?.Error}");
                InitializeFailed temp = new InitializeFailed();
                temp.SendEvent();
            }
            else
            {
                machine.ChangeState<FsmRequestPackageVersion>();
            }
        }
        
        public static readonly string HostServerIP = "http://127.0.0.1";
        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            string appVersion = GameInit.Instance.appVersion;
#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                return $"{HostServerIP}/CDN/Android/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
                return $"{HostServerIP}/CDN/IPhone/{appVersion}";
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
                return $"{HostServerIP}/CDN/WebGL/{appVersion}";
            else
                return $"{HostServerIP}/CDN/PC/{appVersion}";
#else
            if (Application.platform == RuntimePlatform.Android)
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            else
                return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }
    }
}