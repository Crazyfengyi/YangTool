using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YangTools;

namespace GameMain
{
    public class PlatformDefault : IPlatform
    {
        public string OpenId { get; set; }

        public void Initialize()
        {
            PlatformMgr.Instance.Login((result, resultAction) => 
            {
                OpenId = SystemInfo.deviceUniqueIdentifier;
                GetOpenIdSuccess temp = new GetOpenIdSuccess
                {
                    openId = OpenId
                };
                temp.SendEvent();
            });
        }

        public void GameManagerInited()
        {
        }

        public void UnInitialize()
        {
        }

        public void LookAd(Action<bool> result)
        {
            //UIWindowTool.ShowPromptBox("看广告成功");
            Debug.Log("看广告成功");
            result?.Invoke(true);
        }

        public void Shake(string type)
        {
            //UIWindowTool.ShowPromptBox($"震动:{type}");
        }

        public void ShowShare(string str, Action<bool> result)
        {
            result?.Invoke(true);
        }

        public void DirectedShare(int targetId, Action<bool> result)
        {
            result?.Invoke(true);
        }

        public void AddToMiniGameEntrance(Action<bool> result)
        {
            result?.Invoke(true);
        }

        public void CreateDesktopEnterAppIcon()
        {
        }

        public void CheckHaveDesktopAppIcon(Action<bool> callBack)
        {
            callBack?.Invoke(false);
        }

        public void GetCanvasSize(Action<bool, Rect> callBack)
        {
            callBack?.Invoke(true, new Rect(0, 0, Screen.width, Screen.height));
        }

        public void GetUserInfo(bool forceLogin, Action<bool, string, string> result, bool noAuthorityIsCreateBtn,
            Action showBtn, Rect btnPos,
            Action<bool> isLimitBtnCallback)
        {
            result?.Invoke(true, PlatformMgr.DefaultName, "");
        }

        public void HideGetUserBtn()
        {
        }

        public void GetBangsHeight(Action<bool, float> result)
        {
#if UNITY_ANDROID
            // 获取屏幕安全区域的高度（刘海屏区域通常不包括在内）
            Rect safeArea = Screen.safeArea;
            float screenHeight = Screen.height;
            float safeAreaHeight = safeArea.height;
            // 如果安全区域高度小于屏幕总高度，则很可能存在刘海屏
            result?.Invoke(true, screenHeight - safeAreaHeight);
#else
            result?.Invoke(true, 0);
#endif
        }

        public void AddLifeCycleEvent(System.Action hideCallback, System.Action showCallback)
        {
        }

        public void GetGameCircle(RectTransform rectTrans, Vector2 pos, Vector2 canvasRect, Action<object> result)
        {
            result?.Invoke(null);
        }

        public void ShowGameCircleBtn()
        {
        }

        public void HideGameCircleBtn()
        {
        }

        public async Task<string> SetPrivacyShareMessage(Action<bool> callBack)
        {
            callBack?.Invoke(true);
            return "";
        }

        public void TagGameIsRunning()
        {
        }

        public void GetCopyToClipboard(string str, Action<bool> result)
        {
            var te = new TextEditor
            {
                text = str
            };
            te.SelectAll();
            te.Copy();
            result?.Invoke(true);
        }

        public void Report(string eventName, Dictionary<string, string> data)
        {
            Debug.Log(eventName + "打点：" + data);
        }
    }
}