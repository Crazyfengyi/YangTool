#if UNITY_WECHAT_GAME
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameMain.AOT;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using WeChatWASM;

namespace GameMain
{
    public class PlatformWx : IPlatform
    {
        private string WxTag => PlatformMgr.Instance.gameTagWx;
        private string AdUnitId => PlatformMgr.Instance.GetPlatformAdId();
        private string WxAppId => PlatformMgr.Instance.appIdWx;
        public string OpenId { get; set; }
        
        private const string AppSecret = "a51e2d22b6e366028a17f017d1e0941a";
        private WXRewardedVideoAd rewardedVideoAd;
        
        private Action getOpenIdOver;
        private const string SharerId = "SharerId";
        private const string SharerName = "SharerName";
        private const string SharerIcon = "SharerIcon";
        private const string SharerPosType = "SharerPosType";

        public void Initialize()
        {
            PlatformMgr.Instance.Login(WxTag, (result,loginOver) =>
            {
                WX.InitSDK(async (code) =>
                {
                    WX.ReportGameStart();
                    CreateRewardedVideoAd();
                    string openID = await GetUUID();
                    //PlatformMgr.UserId = openID;
                    loginOver?.Invoke();
                    
                    EnterOptionsGame openInfo = WX.GetEnterOptionsSync();
                    Dictionary<string, string> temp = openInfo.query;
                    if (openInfo.referrerInfo != null &&
                        (openInfo.referrerInfo.appId == "1036" || //App分享消息卡片
                         openInfo.referrerInfo.appId == "1007" || //单人聊天会话中的小程序消息卡片 
                         openInfo.referrerInfo.appId == "1008")) //群聊会话中的小程序消息卡片
                    {
                        AuthPrivateMessageOption option = new AuthPrivateMessageOption();
                        option.shareTicket = openInfo.shareTicket;
                        option.success = callbackResult =>
                        {
                            Debug.Log($"测试私密分享:{JsonUtility.ToJson(callbackResult)}");
                        };
                        WX.AuthPrivateMessage(option);
                    }

                    foreach (KeyValuePair<string, string> item in temp)
                    {
                        Debug.Log($"测试定向邀请:{item.Key}:{item.Value}");
                    }
                    Debug.Log($"测试定向邀请2:{JsonUtility.ToJson(openInfo)}");
                });

                WX.OnHide((result) =>
                {
                    LocalSaveMgr.Instance.SaveLocalData(true);
                });
            });
        }
        public void GameManagerInited()
        {
           
        }

        public void UnInitialize() { }

        ///  <summary>
        /// 获取微信 UUID
        ///  </summary>
        public async Task<string> GetUUID()
        {
            var tcs = new TaskCompletionSource<string>();
            WX.Login(new LoginOption()
            {
                success = async (res) =>
                {
                    EnterOptionsGame enterData = WX.GetEnterOptionsSync();
                    getOpenIdOver = () =>
                    {
                        var jsonStr = JsonUtility.ToJson(enterData);
                        Debug.Log($"启动信息:{jsonStr}");
                        if (enterData.query.TryGetValue(SharerId,out string tempSharerId))
                        {
                            // 获取分享者的各项信息
                            string sharerId = enterData.query[SharerId];
                            string sharerName = enterData.query[SharerName];
                            string sharerIcon = enterData.query[SharerIcon];
                            string sharerPosType = enterData.query[SharerPosType];
                            
                            Debug.Log($"分享者信息:{sharerId},{PlatformMgr.UserId}，{sharerName},{sharerIcon},{sharerPosType}");
                            var temp = new HyzShareManager.HyzSharedRoleInfo()
                            {
                                shareUuid = sharerId,
                                currentUuid = PlatformMgr.UserId, //当前登录用户uuid--被分享者
                                nickName = sharerName, //被分享者的昵称
                                iconUrl = sharerIcon, //头像
                                catId = "1", //猫ID
                                posType = int.Parse(sharerPosType), //位置类型
                            };
                            HyzShareManager.Instance.UpLoadSharedInfo(temp);
                        }
                    };
                    
                    Debug.Log($"微信获取code成功:{res.code}");
                    
                    //本地缓存ID
                    string localOpenId = PlayerPrefs.GetString(Constant.OpenID);
                    if (string.IsNullOrEmpty(localOpenId))
                    {
                        string appID = PlatformMgr.Instance.appIdWx;
                        var isOver = false;
                        HyzToolManager.Instance.GetWechatUUID(appID,AppSecret,res.code, (tempRes) =>
                        {
                            string openId = tempRes.openid.ToString();
                            UniTask.SwitchToMainThread();
                            PlayerPrefs.SetString(Constant.OpenID, openId);
                            PlayerPrefs.Save();
                            OpenId = openId;
                            PlatformEventDefine.SendGetOpenIdSuccess(openId);
                            getOpenIdOver?.Invoke();
                            isOver = true;
                            Debug.LogWarning($"获取微信OpenId成功:{openId}");
                            tcs.SetResult("");
                        }, () =>
                        {
                            Debug.LogError("获取微信OpenId失败");
                            isOver = true;
                            tcs.SetResult("");
                        });
                        
                        // 等待请求完成
                        while (!isOver)
                        {
                            await Task.Yield(); //让出控制权，直到请求完成
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"存档openid:{localOpenId}");
                        OpenId = localOpenId;
                        PlatformEventDefine.SendGetOpenIdSuccess(localOpenId);
                        getOpenIdOver?.Invoke();
                        tcs.SetResult("");
                    }
                },
                fail = (err) =>
                {
                    Debug.Log($"微信获取code失败:{err.errMsg}");
                    tcs.SetResult("");
                }
            });
            return await tcs.Task;
        }
        
        /// <summary>
        /// 创建广告
        /// </summary>
        private void CreateRewardedVideoAd()
        {
            rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = AdUnitId,
            });
            
            rewardedVideoAd.OnLoad((res) =>
            {
                Debug.Log("_rewardedVideoAd.OnLoad:" + JsonUtility.ToJson(res));
                
                var reportShareBehaviorRes = rewardedVideoAd.ReportShareBehavior(new RequestAdReportShareBehaviorParam()
                {
                    operation = 1,
                    currentShow = 1,
                    strategy = 0,
                    shareValue = (int) res.shareValue,
                    rewardValue = (int) res.rewardValue,
                    depositAmount = 100,
                });
                
                Debug.Log("ReportShareBehavior.Res:" + JsonUtility.ToJson(reportShareBehaviorRes));
            });
            
            rewardedVideoAd.OnError((err) =>
            {
                Debug.Log("_rewardedVideoAd.OnError:" + JsonUtility.ToJson(err));
            });
            
            rewardedVideoAd.OnClose((res) =>
            {
                Debug.Log("_rewardedVideoAd.OnClose:" + JsonUtility.ToJson(res));
            });
            
            rewardedVideoAd.Load();
        }
        /// <summary>
        /// 看广告
        /// </summary>
        public void LookAd(Action<bool> result)
        {
            result?.Invoke(true);
            return;
            rewardedVideoAd.Show((response) => { },
                (response) =>
                {
                    UIWindowTool.ShowPromptBox("加载广告失败");
                });
            rewardedVideoAd.onCloseAction = (closeResponse) =>
            {
                result?.Invoke(closeResponse.isEnded);
            };
        }
        /// <summary>
        /// 震动
        /// </summary>
        public void Shake(string type)
        {
            WX.VibrateShort(new VibrateShortOption(){type = type});
        }

        /// <summary>
        /// 分享
        /// </summary>
        public void ShowShare(string str, Action<bool> result)
        {
            shareCallback = result;
            WX.ShareAppMessage(new ShareAppMessageOption() {
                title = str,
                //imageUrl = $"{FsmInitializePackage.hostServerIP}/CDN/Common/Share.png",
            });
        }

        public void DirectedShare(string targetId, Action<bool> result)
        {
        }

        public void AddToMiniGameEntrance(Action<bool> result)
        {
        }

        public void CreateDesktopEnterAppIcon()
        {
        }

        public void CheckHaveDesktopAppIcon(Action<bool> callBack)
        {
            callBack?.Invoke(true);
        }

        public void GetCanvasSize(Action<bool, Rect> callBack)
        {
            WindowInfo window = WX.GetWindowInfo();
            var wight = (int) (window.windowWidth * window.pixelRatio);
            var height = (int) (window.windowHeight * window.pixelRatio);
            Debug.LogFormat($"屏幕宽高：{wight},{height}");
            callBack?.Invoke(true, new Rect((float)window.pixelRatio, (float)window.pixelRatio, wight, height));
        }
        private WXUserInfoButton userInfoBtn;
        /// <summary>
        /// 获得用户信息
        /// </summary>
        public void GetUserInfo(bool forceLogin,Action<bool, string, string> result,bool noAuthorityIsCreateBtn, Action showBtn,Rect btnPos,Action<bool> isLimitBtnCallback)
        {
            var settingOption = new GetSettingOption() {
                success = (successResult) =>
                {
                    if (successResult.authSetting.ContainsKey("scope.userInfo"))
                    {
                        //已经授权
                        var userInfoOption = new GetUserInfoOption() {
                            lang = "zh_CN",
                            withCredentials = false,
                            success = (userInfo) =>
                            {
                                result?.Invoke(true, userInfo.userInfo.nickName, userInfo.userInfo.avatarUrl);
                            },
                            fail = (failResult) =>
                            {
                                Debugger.LogError($"获取微信用户信息失败 1:{failResult.errMsg}");
                                result?.Invoke(false, PlatformMgr.DefaultName, PlatformMgr.DefaultIconUrl);
                            }
                        };
                        WX.GetUserInfo(userInfoOption);
                    }
                    else
                    {
                        if (noAuthorityIsCreateBtn)
                        {
                            //没有授权
                            showBtn?.Invoke();
                            userInfoBtn = WX.CreateUserInfoButton((int)btnPos.x, (int)btnPos.y, (int)btnPos.width, (int)btnPos.height, "zh_CN", false);
                            userInfoBtn.Show();
                            userInfoBtn.OnTap((data) =>
                            {
                                if (data.errCode == 0)
                                {
                                    userInfoBtn.Hide();
                                    Debugger.LogWarning($"用户同意授权用户信息,NickName:{data.userInfo.nickName} avatarUrl:{data.userInfo.avatarUrl}");
                                    result?.Invoke(true, data.userInfo.nickName, data.userInfo.avatarUrl);
                                    isLimitBtnCallback?.Invoke(true);
                                }
                                else
                                {
                                    userInfoBtn.Hide();
                                    Debugger.LogWarning("用户拒绝授权用户信息");
                                    result?.Invoke(false, PlatformMgr.DefaultName,PlatformMgr.DefaultIconUrl);
                                    isLimitBtnCallback?.Invoke(false);
                                }
                            });
                        }
                        else
                        {
                            Debugger.LogWarning("用户未授权用户信息");
                            result?.Invoke(false, PlatformMgr.DefaultName,PlatformMgr.DefaultIconUrl);
                        }
                    }
                },
                
                fail = (failResult) =>
                {
                    Debugger.LogError($"获取微信用户信息失败 2:{failResult.errMsg}");
                    result?.Invoke(false, PlatformMgr.DefaultName, PlatformMgr.DefaultIconUrl);
                }
            };
            
            WX.GetSetting(settingOption);
        }
        public void HideGetUserBtn()
        {
            userInfoBtn?.Hide();
        }
        public void GetBangsHeight(Action<bool, float> result)
        {
            GetSystemInfoOption settingOption = new GetSystemInfoOption();
            settingOption.success = (settingResult) =>
            {
                result?.Invoke(true,(float)settingResult.safeArea.top);
            };
            settingOption.fail = (settingResult) =>
            {
                result?.Invoke(false,0);
            };
                
            WX.GetSystemInfo(settingOption);
        }
        
        private Action<bool> shareCallback;
        private DateTime startTime;
        /// <summary>
        /// 添加生命周期事件
        /// </summary>
        public void AddLifeCycleEvent(Action hideCallback, Action showCallback)
        {
            WX.OnHide((res) => { hideCallback?.Invoke(); });

            WX.OnShow((res) =>
            {
                Debug.Log($"OnShowOneParam:${JsonUtility.ToJson(res)}");
                showCallback?.Invoke();
                if (shareCallback != null)
                {
                    TimeSpan interval = DateTime.UtcNow - startTime;
                    //分享页面停留2s往上就算分享成功
                    if (interval.TotalSeconds >= 2)
                    {
                        Debug.Log($"share succee:{interval.TotalSeconds}");
                        shareCallback?.Invoke(true);
                    }
                    else
                    {
                        Debug.Log($"share fail:{interval.TotalSeconds}");
                        shareCallback?.Invoke(false);
                    }
                    shareCallback = null;
                }
            });
        }
        
        private WXGameClubButton wxGameClubButton;
        /// <summary>
        /// 游戏圈
        /// </summary>
        public void GetGameCircle(RectTransform rectTransform, Vector2 pos, Vector2 canvasRect, Action<object> result)
        {
            if (wxGameClubButton != null)
            {
                result?.Invoke(wxGameClubButton);
                return;
            }

            WXCreateGameClubButtonParam param = new WXCreateGameClubButtonParam();
            param.type = GameClubButtonType.text;
            param.text = "";
            param.icon = GameClubButtonIcon.dark;
            param.hasRedDot = false;
            //param.openlink = "";

            GameClubButtonStyle gameClubButtonStyle = new GameClubButtonStyle();
            //微信按钮的中心点在左上角
            pos.y = -pos.y;
            int posX = (int) (pos.x - rectTransform.rect.width / 2);
            int posY = (int) (pos.y - rectTransform.rect.height / 2);
            Debug.Log($"测试0:{posX}:{rectTransform.rect.width}");

            float wRadion = (float) WX.GetSystemInfoSync().screenWidth / canvasRect.x;
            float hRadion = (float) WX.GetSystemInfoSync().screenHeight / canvasRect.y;
            //WX.GetSystemInfoSync().screenHeight 实际高度 844 未算dpi
            //Screen.height 实际高度 2532
            gameClubButtonStyle.left = (int) (posX * wRadion);
            gameClubButtonStyle.top = (int) (posY * hRadion);
            gameClubButtonStyle.width = (int) (rectTransform.rect.width * wRadion);
            gameClubButtonStyle.height = (int) (rectTransform.rect.height * hRadion);
            param.style = gameClubButtonStyle;

            Debug.Log($"测试:{posX}:{posY}:{hRadion}:{wRadion}");
            Debug.Log(
                $"测试2:{gameClubButtonStyle.left}:{gameClubButtonStyle.top}:{gameClubButtonStyle.width}:{gameClubButtonStyle.height}");
            wxGameClubButton = WX.CreateGameClubButton(param);
            wxGameClubButton.Show();
            result?.Invoke(wxGameClubButton);
        }

        public void ShowGameCircleBtn()
        {
            wxGameClubButton?.Show();
        }

        public void HideGameCircleBtn()
        {
            wxGameClubButton?.Hide();
        }
        
        /// <summary>
        /// 设置私密分享信息
        /// </summary>
        public async Task<string> SetPrivacyShareMessage(Action<bool> callBack)
        {
            string uri = $"https://api.weixin.qq.com/cgi-bin/message/wxopen/activityid/create?access_token={AppSecret}";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                WeixinPrivacyShareRequest request = new WeixinPrivacyShareRequest();
                request.openid = PlatformMgr.UserId;
                byte[] bodyRaw = Encoding.UTF8.GetBytes(request.ToString());
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                // 发送请求并等待响应
                var operation = webRequest.SendWebRequest();
                // 等待请求完成
                while (!operation.isDone)
                {
                    await Task.Yield(); //让出控制权，直到请求完成
                }

                // 检查是否有错误
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"检查错误: {JsonUtility.ToJson(webRequest.error)}");
                }
                else
                {
                    Debug.Log("返回结果");
                    WeixinPrivacyShareResult result = JsonUtility.FromJson<WeixinPrivacyShareResult>(webRequest.downloadHandler.text);
                    UpdateShareMenuOption temp = new UpdateShareMenuOption();
                    temp.withShareTicket = true;
                    temp.isPrivateMessage = true;
                    temp.activityId = result.activityId;
                    temp.templateInfo = new UpdatableMessageFrontEndTemplateInfo();
                    temp.templateInfo.templateId = PlatformMgr.UserId;
                    temp.success = result =>
                    {
                        Debug.Log($"UpdateShareMenuOption success:{JsonUtility.ToJson(result)}");
                        callBack?.Invoke(true);
                    };
                    temp.fail = result =>
                    {
                        Debug.Log($"UpdateShareMenuOption fail:{JsonUtility.ToJson(result)}");
                    };
                    WX.UpdateShareMenu(temp);
                }
            }
            //三、修改动态消息内容
            //动态消息发出去之后，可以通过 setUpdatableMsg 修改消息内容。
            return "";
        }
        public void TagGameIsRunning()
        {
            ReportSceneOption option = new ReportSceneOption();
            option.sceneId = 7;
            option.costTime = 1;
            WX.ReportScene(option);
        }

        public void GetCopyToClipboard(string str, Action<bool> result)
        {
           
        }

        public void Report(string eventName, object data)
        {
           
        }

        public void DirectedShare(int posType, Action<bool> result)
        {
            throw new NotImplementedException();
        }

        public void Report(string eventName, Dictionary<string, string> data)
        {
            throw new NotImplementedException();
        }
    }
    
    public class WeixinLoginResult
    {
        public string session_key;
        public string openid;
    }
    public class WeixinPrivacyShareRequest
    {
        public string unionid;
        public string openid;
    }
    
    public class WeixinPrivacyShareResult
    {
        public string activityId;
        public string expiration_time;
        public string errcode;
        public string errmsg;
    }
}
#endif
