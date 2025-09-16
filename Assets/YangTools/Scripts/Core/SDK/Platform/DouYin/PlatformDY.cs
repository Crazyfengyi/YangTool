#if UNITY_DOUYIN_GAME
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

using TTSDK.UNBridgeLib.LitJson;
using Newtonsoft.Json;
using StarkSDKSpace;
using TTSDK;
using LaunchOption = TTSDK.LaunchOption;

namespace GameMain
{
    public class PlatformDy : IPlatform
    {
        public string OpenId { get; set; }
        private const string AppSecret = "xxxxx";

        /// <summary>
        /// 系统标签
        /// </summary>
        public string SystemTag;

        #region 广告进入

        private const string AdsEnterTag = "ad_params";

        #endregion

        #region 分享信息

        /// <summary>
        /// 分享者ID
        /// </summary>
        private const string SharerId = "SharerId";

        private const string SharerName = "SharerName";
        private const string SharerIcon = "SharerIcon";
        private const string SharerPosType = "SharerPosType";

        #endregion

        private string DyAppId => PlatformMgr.Instance.appIdDy;
        private string DySeggTag => PlatformMgr.Instance.gameTagDy;

        private TTRewardedVideoAd rewardAd;
        private Action<bool, int> adCloseCallBack;

        private Action getOpenIdOver;
        private string adParamStr; //广告进入数据

        // ReSharper disable Unity.PerformanceAnalysis
        public void Initialize()
        {
            PlatformMgr.Instance.Login(DySeggTag, (loginResult, result) =>
            {
                TT.InitSDK((code, data) =>
                {
                    LaunchOption enterData = TT.GetLaunchOptionsSync();
                    TTSystemInfo window = TT.GetSystemInfo();
                    SystemTag = window.platform;

                    getOpenIdOver = () =>
                    {
                        if (enterData.Query.TryGetValue(SharerId, out var sharerId))
                        {
                            string sharerName = enterData.Query[SharerName];
                            string sharerIcon = enterData.Query[SharerIcon];
                            string sharerPosType = enterData.Query[SharerPosType];

                            var temp = new HyzShareManager.HyzSharedRoleInfo()
                            {
                                shareUuid = sharerId,
                                currentUuid = PlatformMgr.UserId, //当前登录用户uuid--被分享者
                                nickName = sharerName, //被分享者的昵称
                                iconUrl = DouYinUtility.Decode(sharerIcon), //头像
                                //catId = LocalSaveMgr.Instance.DataCenter.GetSaveMainUIData().PartSelfCatId.ToString(), //猫ID
                                posType = int.Parse(sharerPosType), //位置类型
                            };
                            /*if (int.TryParse(sharerPosType,out var typeId) && typeId == (int)HyzShareManager.SharePosType.InvitePos)
                            {
                                MainUIDataManager.Instance.AddInviteCount(1);
                            }*/
                            HyzShareManager.Instance.UpLoadSharedInfo(temp);
                        }
                    };

                    #region 启动场景

                    if (enterData != null && enterData.Scene != null)
                    {
                        Debug.Log($"启动场景:{enterData.Scene}");
                        /*  061004 我的-小程序列表-最近使用
                            181003 我的页面-常用功能-小游戏 list
                            101001 抖极个人主页侧边栏
                            011004 我的-小程序列表-最近使用
                            011006 我的-小程序列表-发现推荐
                            021001 抖音个人主页侧边栏
                        */
                        switch (enterData.Scene)
                        {
                            case "061004":
                            case "181003":
                            case "101001":
                            case "011004":
                            case "011006":
                            case "021001":
                                PlatformMgr.IsMiniGameEntrance = true;
                                break;
                        }
                    }

                    #endregion

                    #region 广告进入

                    if (enterData != null && enterData.Query.TryGetValue(AdsEnterTag, out var adsStr))
                    {
                        string jsonStr = DouYinUtility.Decode(adsStr); //解码
                        Debug.LogError($"有广告点击参数2:{jsonStr}");
                        adParamStr = jsonStr;
                    }

                    #endregion

                    #region 获取OpenId

                    OpenId = PlayerPrefs.GetString(Constant.OpenID, OpenId);
                    if (string.IsNullOrEmpty(OpenId))
                    {
                        Debugger.LogWarning("获取OpenId");
#if UNITY_EDITOR
                        OpenId = SystemInfo.deviceUniqueIdentifier;
                        PlatformEventDefine.SendGetOpenIdSuccess(OpenId);
#else
                        TT.Login(LoginSuccess, LoginFailed, false);
#endif
                    }
                    else
                    {
                        Debug.LogWarning($"存档openid:{OpenId}");
                        PlatformEventDefine.SendGetOpenIdSuccess(OpenId);
                        getOpenIdOver?.Invoke();
                    }

                    #endregion

                    #region 后台Log

                    string str = "";
                    if (enterData != null)
                    {
                        for (int i = 0; i < enterData.Query.Count; i++)
                        {
                            KeyValuePair<string, string> temp = enterData.Query.ElementAt(i);
                            Debug.LogWarning($"有参数:{temp.Key}--{temp.Value}");
                            str += $"有参数:{temp.Key}--{temp.Value}";
                        }

                        GetRealtimeManagerParam tempData = new GetRealtimeManagerParam();
                        TTRealtimeLogManager logger = TT.GetRealtimeLogManager(tempData);
                        logger.Warn($"测试定向分享:{str}");
                    }

                    #endregion

                    #region 广告

                    string adId = PlatformMgr.Instance.GetPlatformAdId();
                    rewardAd = TT.CreateRewardedVideoAd(adId, (adLookResult, resultCode) =>
                    {
                        Debug.Log($"广告播放完成:{adLookResult}");
                        adCloseCallBack?.Invoke(adLookResult, resultCode);
                    }, (errCode, errStr) =>
                    {
                        Debug.Log($"广告播放出错:{errStr}，{errCode}");
                        adCloseCallBack?.Invoke(false, errCode);
                    });
                    rewardAd.Load();

                    #endregion
                });
            });

            TT.GetAppLifeCycle().OnHide += OnAppHideEventHide;
            //返回true false 都会立即关闭游戏
            TT.SetOnBeforeExitAppListener(OnBeforeExit);
        }

        private bool OnBeforeExit()
        {
            PlatformEventDefine.SendSaveDataToServer();
            return false;
        }

        private void OnAppHideEventHide()
        {
            PlatformEventDefine.SendSaveDataToServer();
        }

        public void GameManagerInited()
        {
            if (LocalSaveMgr.Instance.DataCenter.GetLocalSave<Save_Setting>().firstEnterGameFlag)
            {
                LocalSaveMgr.Instance.DataCenter.GetLocalSave<Save_Setting>().isAdsUser =
                    !String.IsNullOrEmpty(adParamStr);
                LocalSaveMgr.Instance.DataCenter.GetLocalSave<Save_Setting>().adsInfo =
                    JsonUtility.FromJson<DouYinAdsInfo>(adParamStr);
                Debug.LogError($"是否为广告进入用户:{LocalSaveMgr.Instance.DataCenter.GetLocalSave<Save_Setting>().isAdsUser}");
            }
        }

        public void UnInitialize()
        {
            TT.GetAppLifeCycle().OnHide -= OnAppHideEventHide;
        }

        private void LoginSuccess(string code, string anonymousCode, bool isLogin)
        {
            GameEntrance.Instance.StartCoroutine(GetDouYinOpenId(code, anonymousCode));
        }

        private IEnumerator GetDouYinOpenId(string code, string anonymousCode)
        {
            Debugger.LogWarning($"抖音获取code成功:{code}");
            string appID = PlatformMgr.Instance.appIdDy;
            string url =
                $"https://minigame.zijieapi.com/mgplatform/api/apps/jscode2session?appid={appID}&secret={AppSecret}&code={code}";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 30;
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debugger.LogError("获取抖音openid错误");
                    yield break;
                }

                var response = webRequest.downloadHandler.text;
                Debugger.LogWarning($"获取抖音openid返回:{response}");
                var douYinOpenId = JsonConvert.DeserializeObject<DouYinOpenId>(response);
                if (!string.IsNullOrEmpty(douYinOpenId.openid))
                {
                    OpenId = douYinOpenId.openid;
                    Debug.LogWarning($"获取抖音openid成功:{OpenId}");
                    PlayerPrefs.SetString(Constant.OpenID, OpenId);
                    PlayerPrefs.Save();
                    PlatformEventDefine.SendGetOpenIdSuccess(OpenId);
                    getOpenIdOver?.Invoke();
                }

                yield return null;
            }
        }

        private void LoginFailed(string errMsg)
        {
            Debugger.LogError($"登录抖音失败 errMsg={errMsg}");
        }

        public void LookAd(Action<bool> result)
        {
            adCloseCallBack = (resultBool, code) => { result?.Invoke(resultBool); };
            rewardAd.Show();
            rewardAd.Load();
        }

        /// <summary>
        /// WebGL 平台下，只有两种震动：长和短。
        /// pattern数组参数只能传入一个数值，传入小于1000 则为短震动，否则为长震动。
        /// 不支持取消和重复。即repeat参数无效。
        /// 当收不到长震动时，可以检查是否在 IOS 开启静音模式了。静音模式下，长震动会被禁。
        /// </summary>
        public void Shake(string type)
        {
            //震动 heavy、medium、light--->高、中、低
            long[] pattern = new long[] { 100 };
            switch (type)
            {
                case "heavy":
                    pattern = new long[] { 2000 };
                    break;
                case "medium":
                case "light":
                    pattern = new long[] { 100 };
                    break;
            }

            TT.Vibrate(pattern);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void ShowShare(string str, Action<bool> result)
        {
            JsonData shareJson = new JsonData
            {
                ["channel"] = "invite",
                ["desc"] = string.IsNullOrEmpty(str) ? "有本事就把第5关过了" : str,
                ["imageUrl"] = "",
            };

            TT.ShareAppMessage(shareJson, (resultDic) =>
            {
                result?.Invoke(true); //成功
                Debug.Log("分享成功");
            }, (errorStr) =>
            {
                result?.Invoke(false); //失败
                Debug.Log("分享失败");
            }, () =>
            {
                result?.Invoke(false); //关闭
            });
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 定向分享
        /// </summary>
        /// <param name="result"></param>
        public void DirectedShare(int posType, Action<bool> result)
        {
            JsonData shareJson = new JsonData
            {
                ["channel"] = "invite",
                ["desc"] = "有本事就把第5关过了",
                ["imageUrl"] = "",
                ["query"] =
                    $"{SharerId}={PlatformMgr.UserId}&{SharerName}={PlatformMgr.NickName}&{SharerIcon}={DouYinUtility.EnCode(PlatformMgr.IconUrl)}&{SharerPosType}={posType}",
            };

            TT.ShareAppMessage(shareJson, (resultDic) =>
            {
                result?.Invoke(true); //成功
                Debug.Log("分享成功");
            }, (errorStr) =>
            {
                result?.Invoke(false); //失败
                Debug.Log("分享失败");
            }, () =>
            {
                result?.Invoke(false); //关闭
            });
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void AddToMiniGameEntrance(Action<bool> result)
        {
            JsonData data = new JsonData
            {
                ["scene"] = "sidebar",
            };
            TT.NavigateToScene(data, () =>
            {
                result?.Invoke(true);
                Debug.Log($"添加侧边栏成功");
            }, () => { }, (errCode, errMsg) =>
            {
                result?.Invoke(false);
                Debug.LogError($"添加侧边栏报错:{errCode},{errMsg}");
            });
        }

        public void CreateDesktopEnterAppIcon()
        {
            //目前仅支持抖音和抖 lite,请在其它app做屏蔽.该 API必须要由用户点击触发
            TT.AddShortcut((str) => { Debug.Log("添加桌面成功"); });
        }

        public void CheckHaveDesktopAppIcon(Action<bool> callBack)
        {
#if UNITY_ANDROID
            //目前仅支持抖音和抖 lite，请在其它 app 做屏蔽.仅在Android上支持
            TT.CheckShortcut((result) =>
            {
                Debug.Log($"检查添加到桌面:{result}");
                callBack?.Invoke(result);
            });
#else
            callBack?.Invoke(false);
#endif
        }

        public void AddLifeCycleEvent(System.Action hideCallback, System.Action showCallback)
        {
            TT.GetAppLifeCycle().OnHide += () => { hideCallback?.Invoke(); };
            TT.GetAppLifeCycle().OnShow += (param) =>
            {
                Debug.Log($"OnShowOneParam:{param}");
                showCallback?.Invoke();
            };
        }

        public void GetCanvasSize(Action<bool, Rect> callBack)
        {
            TTSystemInfo window = TT.GetSystemInfo();
            var wight = (int)(window.screenWidth * window.pixelRatio);
            var height = (int)(window.screenHeight * window.pixelRatio);
            Debug.LogFormat($"屏幕宽高:{wight},{height}");
            callBack?.Invoke(true, new Rect((float)window.pixelRatio, (float)window.pixelRatio, wight, height));
        }

        public void GetUserInfo(bool forceLogin, Action<bool, string, string> result, bool noAuthorityIsCreateBtn,
            Action showBtn, Rect btnPos, Action<bool> isLimitBtnCallback)
        {
            if (!forceLogin)
            {
                if (PlayerPrefs.HasKey("DouYinNickNameKey") && PlayerPrefs.HasKey("DouYinIconUrlKey"))
                {
                    result?.Invoke(false, PlayerPrefs.GetString("DouYinNickNameKey"),
                        PlayerPrefs.GetString("DouYinIconUrlKey"));
                }
                else
                {
                    result?.Invoke(false, PlatformMgr.DefaultName, PlatformMgr.DefaultIconUrl);
                }

                return;
            }

            TT.Login((string code, string anonymousCode, bool isLogin) =>
                {
                    TT.GetUserInfo(false, false, (ref TTUserInfo scUserInfo) =>
                    {
                        PlayerPrefs.SetString("DouYinNickNameKey", scUserInfo.nickName);
                        PlayerPrefs.SetString("DouYinIconUrlKey", scUserInfo.avatarUrl);
                        PlayerPrefs.Save();
                        result?.Invoke(true, scUserInfo.nickName, scUserInfo.avatarUrl);
                    }, (string msg) => { result?.Invoke(false, PlatformMgr.DefaultName, PlatformMgr.DefaultIconUrl); });
                }, (string errMsg) => { result?.Invoke(false, PlatformMgr.DefaultName, PlatformMgr.DefaultIconUrl); },
                false);
        }

        public void GetGameCircle(RectTransform rectTrans, Vector2 pos, Vector2 canvasRect, Action<object> result)
        {
        }

        public void ShowGameCircleBtn()
        {
        }

        public void HideGameCircleBtn()
        {
        }

        public async Task<string> SetPrivacyShareMessage(Action<bool> callBack)
        {
            return "";
        }

        public void HideGetUserBtn()
        {
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void GetBangsHeight(Action<bool, float> result)
        {
            TTSystemInfo settingOption = TT.GetSystemInfo();
            Debug.Log(
                $"刘海测试:{settingOption.safeArea.ToString()}---{settingOption.safeArea.height}--{settingOption.statusBarHeight}");
            result?.Invoke(true, (float)settingOption.safeArea.top + 30);
        }

        public void TagGameIsRunning()
        {
            JsonData ob = new JsonData();
            ob["sceneId"] = 7001;
            ob["costTime"] = 10;
            TT.ReportScene(ob);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void GetCopyToClipboard(string str, Action<bool> result)
        {
            TT.SetClipboardData(str, (b, s) =>
            {
                if (b)
                {
                    result?.Invoke(true);
                    Debug.Log("设置成功");
                }
                else
                {
                    result?.Invoke(false);
                    Debug.Log("设置失败，" + s);
                }
            });
        }


        /// <summary>
        /// 体力恢复场景
        /// </summary>
        /// <param name="Success"></param>
        /// <param name="Failed"></param>
        public void RequestFeedSubscribePower(Action<JsonData> Success, Action<int, string> Failed)
        {
            /*var contentIDs = JsonData.NewJsonArray();
            contentIDs
                .Add("CONTENT12491529986");*/
            /*var param = new JsonData
            {
                ["type"]="play", // play=直玩
                ["scene"]=2, // 一次只能订阅一个
                ["contentIDs"]=contentIDs,
            };*/
            var param = new JsonData
            {
                ["type"] = "play", // play=直玩
                ["allScene"] = true, // 全场景订阅
            };
            TT.RequestFeedSubscribe(
                param,
                (res) => { Success?.Invoke(res); },
                (errNo, errMsg) => { Failed?.Invoke(errNo, errMsg); },
                () => { }
            );
        }


        public void RequestSubscribeMessage(Action<Dictionary<string, string>> Success, Action<int, string> Failed)
        {
            List<string> list = new List<string>();
            list.Add("MSG2187905137187535678696808548646");
            TT.RequestSubscribeMessage(list,
                (res) => { Success?.Invoke(res); },
                () => { },
                (retCode, retError) => { Failed?.Invoke(retCode, retError); });
        }

        /// <summary>
        /// 查询体力订阅状态
        /// </summary>
        /// <param name="Success"></param>
        /// <param name="Failed"></param>
        public void CheckFeedSubscribePowerStatus(Action<JsonData> Success, Action<int, string> Failed)
        {
            var param = new JsonData
            {
                ["type"] = "play",
                ["scene"] = 2,
            };

            TT.CheckFeedSubscribeStatus(
                param,
                (res) => { Success?.Invoke(res); },
                (errNo, errMsg) => { Failed?.Invoke(errNo, errMsg); },
                () => { }
            );
        }

        /// <summary>
        /// 添加直玩场景内外流的切换的监听。
        /// </summary>
        /// <param name="callback"></param>
        public void OnFeedStatusChange(OnFeedStatusChangeCallback callback)
        {
            TT.OnFeedStatusChange(callback);
        }


        public void Report(string eventName, Dictionary<string, string> data)
        {
            TT.ReportAnalytics(eventName, data);
        }
    }

    public class DouyinQueryData
    {
        public string sharerUUID;
    }

    [System.Serializable]
    public class DouYinOpenId
    {
        public string anonymous_openid;

        public string openid;

        public int error;

        public string session_key;

        public string unionid;
    }
}
#endif
