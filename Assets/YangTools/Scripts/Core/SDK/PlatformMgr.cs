using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using YangTools;

namespace GameMain
{
    public class PlatformMgr : SerializedMonoBehaviour, IPlatform
    {
        public static PlatformMgr Instance { get; private set; }

        [LabelText("平台")] [OnValueChanged("OnPlatformChange")]
        public PlatformType platformType;
        [LabelText("平台数据")] 
        public Dictionary<PlatformType, PlatformData> platformData;
        
        public string PlatformAppId => platformData.TryGetValue(platformType, out var value) ? value.appId : null;
        public string PlatformAdId => platformData.TryGetValue(platformType, out var value) ? value.adsId : null;
        public string PlatformAppKey => platformData.TryGetValue(platformType, out var value) ? value.appKey : null;
        public string PlatformAppSecret => platformData.TryGetValue(platformType, out var value) ? value.appSecret : null;
        public string PlatformGameTag => platformData.TryGetValue(platformType, out var value) ? value.gameTag : null;

        private IPlatform platform;
        public IPlatform Platform => platform;
        
        /// <summary>
        /// 用户唯一ID
        /// </summary>
        public string OpenId
        {
            get => platform?.OpenId;
            set => platform.OpenId = value;
        }

        [LabelText("跳过看广告")] 
        public bool skipLookAd;

        /// <summary>
        /// 小程序进入
        /// </summary>
        public static bool IsMiniGameEntrance;

        /// <summary>
        /// 获得平台类
        /// </summary>
        private IPlatform NewPlatform()
        {
#if UNITY_EDITOR
            return new PlatformDefault();
#elif UNITY_DOUYIN_GAME
            return new PlatformDy();
#elif UNITY_WECHAT_GAME
            return new PlatformWx();
#else
            return new PlatformDefault();
#endif
        }
        
        protected void Awake()
        {
            Instance = this;
            Initialize();
        }

        public void OnDestroy()
        {
            UnInitialize();
        }

        public void Initialize()
        {
            platform = NewPlatform();
            platform.Initialize();
            Debug.Log($"初始化平台成功:{platform.GetType()}");
            switch (platformType)
            {
                case PlatformType.WeiXin:
#if !UNITY_EDITOR && PP_WX
                    SystemInfo systemInfo = WX.GetSystemInfoSync();
                    Debug.Log($"系统：{systemInfo?.platform}");
                    if (systemInfo != null && systemInfo.platform == "ios")
                    {
                        Debug.Log("苹果设置成24帧");
                        Application.targetFrameRate = 24;
                        WX.SetPreferredFramesPerSecond(24);
                    }
#endif
                    break;
                case PlatformType.DouYin:
                    Application.targetFrameRate = 24;
                    break;
                case PlatformType.Default:
                    Application.targetFrameRate = 45;
                    break;
            }
        }

        public void UnInitialize()
        {
            Debug.Log($"销毁平台成功:{platform.GetType()}");
            platform?.UnInitialize();
            platform = null;
        }

        public void GameManagerInited()
        {
            platform.GameManagerInited();
        }

        /// <summary>
        /// 播放广告
        /// </summary>
        public void LookAd(Action<bool> result, string lookAdType)
        {
            if (skipLookAd)
            {
                result?.Invoke(true);
            }
            else
            {
                platform.LookAd((lookAdResult) =>
                {
                    result?.Invoke(lookAdResult);

                    if (!lookAdResult)
                    {
                        //UIWindowTool.ShowPromptBox("未看完广告，无法获取奖励");
                        Debug.Log("未看完广告，无法获取奖励");
                    }
                });
            }
        }

        void IPlatform.LookAd(Action<bool> result)
        {
        }

        /// <summary>
        /// 震动heavy、medium、light
        /// </summary>
        public void Shake(string type = "medium")
        {
            //heavy、medium、light
            //if (LocalSaveMgr.Instance.DataCenter.GetLocalSave<Save_Setting>().shakeIsOn == false) return;
            platform.Shake(type);
        }

        /// <summary>
        /// 分享
        /// </summary>
        public void ShowShare(string str = "", Action<bool> result = null)
        {
            platform?.ShowShare(str, (success) =>
            {
                result?.Invoke(success);
            });
        }

        /// <summary>
        /// 定向邀请
        /// </summary>
        public void DirectedShare(int posType, Action<bool> result)
        {
            platform.DirectedShare(posType, (resultB) =>
            {
                result?.Invoke(resultB);
            });
        }

        public void AddToMiniGameEntrance(Action<bool> result)
        {
            platform.AddToMiniGameEntrance(result);
        }

        /// <summary>
        /// 创建桌面快捷方式
        /// </summary>
        public void CreateDesktopEnterAppIcon()
        {
            platform.CreateDesktopEnterAppIcon();
        }

        /// <summary>
        /// 检测是否有桌面快捷方式
        /// </summary>
        public void CheckHaveDesktopAppIcon(Action<bool> callBack)
        {
            platform?.CheckHaveDesktopAppIcon(callBack);
        }

        /// <summary>
        /// 获得屏幕大小
        /// </summary>
        public void GetCanvasSize(Action<bool, Rect> callBack)
        {
            platform.GetCanvasSize(callBack);
        }

        /// <summary>
        /// 获得用户信息
        /// </summary>
        /// <param name="forceLogin"></param>
        /// <param name="result"></param>
        /// <param name="noAuthorityIsCreateBtn"></param>
        /// <param name="showBtn"></param>
        /// <param name="btnPos"></param>
        /// <param name="isLimitBtnCallback"></param>
        public void GetUserInfo(bool forceLogin, Action<bool, string, string> result, bool noAuthorityIsCreateBtn,
            Action showBtn, Rect btnPos, Action<bool> isLimitBtnCallback)
        {
            platform.GetUserInfo(forceLogin, result, noAuthorityIsCreateBtn, showBtn, btnPos, isLimitBtnCallback);
        }

        public void HideGetUserBtn()
        {
            platform.HideGetUserBtn();
        }

        /// <summary>
        /// 获得刘海高
        /// </summary>
        public void GetBangsHeight(Action<bool, float> result)
        {
            platform.GetBangsHeight(result);
        }

        /// <summary>
        /// 添加生命周期回调
        /// </summary>
        public void AddLifeCycleEvent(System.Action hideCallback, System.Action showCallback)
        {
            platform.AddLifeCycleEvent(hideCallback, showCallback);
        }

        /// <summary>
        /// 获得游戏圈
        /// </summary>
        public void GetGameCircle(RectTransform rectTrans, Vector2 pos, Vector2 canvasRect, Action<object> result)
        {
        }

        /// <summary>
        /// 显示游戏圈
        /// </summary>
        public void ShowGameCircleBtn()
        {
        }

        /// <summary>
        /// 隐藏游戏圈 
        /// </summary>
        public void HideGameCircleBtn()
        {
        }

        /// <summary>
        /// 设置私密分享信息 
        /// </summary>
        public async Task<string> SetPrivacyShareMessage(Action<bool> callBack)
        {
            return "";
        }

        /// <summary>
        /// 标记游戏开始运行
        /// </summary>
        public void TagGameIsRunning()
        {
            platform.TagGameIsRunning();
        }

        /// <summary>
        /// 复制到剪切板
        /// </summary>
        public void GetCopyToClipboard(string str, Action<bool> result)
        {
            platform.GetCopyToClipboard(str, result);
        }

        #region 登录相关

        private int loginCount;
        public static bool IsLoginSuccess;
        private static string userId;
        private static string defaultName = "未知用户";
        private static string defaultIconUrl = "";
        public static string DefaultName => defaultName + userId?.Substring(0, 4);
        public static string DefaultIconUrl => defaultIconUrl;
        
        /// <summary>
        /// 用户显示短ID
        /// </summary>
        public static string UserId => userId;

        /// <summary>
        /// 用户昵称
        /// </summary>
        public static string NickName { get; set; }
        /// <summary>
        /// 用户头像地址
        /// </summary>
        public static string IconUrl { get; set; }

        private readonly YangEventGroup eventGroup = new YangEventGroup();
        public void Login(Action<bool, Action> resultAction)
        {
            eventGroup.AddListener<EventMessageBase>(OnGetOpenIdSuccess);
            resultAction?.Invoke(true, null);
#if !UNITY_EDITOR
            if (ePlatform == EPlatform.Default)
            {
                userId = SystemInfo.deviceUniqueIdentifier;
                IsLoginSuccess = true;
                resultAction?.Invoke(true,null);
                ReportManager.Instance.StartLoad();
                //已有玩家信息,开始加载
                Init.Instance.AutoClickStartBtn();
                NickName = DefaultName;
                IconUrl = defaultIconUrl;
                return;
            }
#endif
        }

        /// <summary>
        /// 获得唯一ID成功
        /// </summary>
        private void OnGetOpenIdSuccess(EventData data)
        {
            if (data is GetOpenIdSuccess)
            {
                IsLoginSuccess = true;
                userId = OpenId.GetHashCode().ToString();
                //ReportManager.Instance.StartLoad();
                //已有玩家信息，开始加载
                //Init.Instance.AutoClickStartBtn();
                //获取用户信息
                GetUserInfo(true, (result, _nickName, _iconUrl) =>
                {
                    Debug.Log($"获取用户信息:{result}昵称nick:{_nickName},头像url:{_iconUrl}");
                    if (result)
                    {
                        NickName = _nickName;
                        IconUrl = _iconUrl;
                    }
                    else
                    {
                        NickName = DefaultName;
                        IconUrl = defaultIconUrl;
                    }
                }, false, null, Rect.zero, null);
            }
        }

        /// <summary>
        /// 打点
        /// </summary>
        public void Report(string eventName, Dictionary<string, string> data)
        {
            platform.Report(eventName, data);
        }

        #endregion

#if UNITY_EDITOR
        
        private void OnPlatformChange()
        {
            //获取当前是哪个平台
            UnityEditor.BuildTargetGroup buildTargetGroup = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
            //获得当前平台已有的宏定义
            var symbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            //另外加一个SDK宏
            symbols = symbols.Replace("UNITY_WECHAT_GAME", "");
            symbols = symbols.Replace("UNITY_DOUYIN_GAME", "");
            symbols = symbols.Replace("UNITASK_DOTWEEN_SUPPORT", "");
            symbols += ";UNITASK_DOTWEEN_SUPPORT";

            switch (platformType)
            {
                case PlatformType.Default:
                    break;
                case PlatformType.WeiXin:
                    //另外加一个SDK宏
                    symbols += ";UNITY_WECHAT_GAME";
                    symbols += ";PF_WX";
                    break;
                case PlatformType.DouYin:
                    symbols += ";UNITY_DOUYIN_GAME";
                    symbols += ";PF_TT";
                    break;
            }
            //重新设置宏
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
        }
#endif
    }
}