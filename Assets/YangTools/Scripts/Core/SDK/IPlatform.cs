using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameMain
{
    public interface IPlatform
    {
        /// <summary>
        /// 用户唯一ID
        /// </summary>
        public string OpenId { get; set; }
        void Initialize();
        void UnInitialize();
        /// <summary>
        /// 游戏管理器初始化完成
        /// </summary>
        void GameManagerInited();
        /// <summary>
        /// 看广告
        /// </summary>
        void LookAd(Action<bool> result);
        /// <summary>
        /// 震动
        /// </summary>
        void Shake(string type);
        /// <summary>
        /// 分享
        /// </summary>
        public void ShowShare(string str,Action<bool> result);
        /// <summary>
        /// 定向分享
        /// </summary>
        public void DirectedShare(int posType,Action<bool> result);
        /// <summary>
        /// 添加到小程序快捷入口
        /// </summary>
        public void AddToMiniGameEntrance(Action<bool> result);
        /// <summary>
        /// 添加到桌面
        /// </summary>
        public void CreateDesktopEnterAppIcon();
        /// <summary>
        /// 检测是否已经添加到桌面 
        /// </summary>
        public void CheckHaveDesktopAppIcon(Action<bool> callBack);
        /// <summary>
        /// 获得屏幕大小
        /// </summary>
        public void GetCanvasSize(Action<bool, Rect> callBack);
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
            Action showBtn, Rect btnPos, Action<bool> isLimitBtnCallback);
        /// <summary>
        /// 隐藏获取用户信息按钮
        /// </summary>
        public void HideGetUserBtn();
        /// <summary>
        /// 获得刘海高度
        /// </summary>
        void GetBangsHeight(Action<bool, float> result);
        /// <summary>
        /// 添加生命周期回调
        /// </summary>
        void AddLifeCycleEvent(System.Action hideCallback, System.Action showCallback);
        /// <summary>
        /// 获得游戏圈
        /// </summary>
        void GetGameCircle(RectTransform rectTrans, Vector2 pos, Vector2 canvasRect, Action<object> result);
        /// <summary>
        /// 显示游戏圈按钮
        /// </summary>
        void ShowGameCircleBtn();
        /// <summary>
        /// 隐藏游戏圈按钮
        /// </summary>
        void HideGameCircleBtn();
        /// <summary>
        /// 设置私密分享信息
        /// </summary>
        Task<string> SetPrivacyShareMessage(Action<bool> callBack);
        /// <summary>
        /// 标记游戏开始运行
        /// </summary>
        void TagGameIsRunning();
        /// <summary>
        /// 复制到剪切板 
        /// </summary>
        void GetCopyToClipboard(string str, Action<bool> result);
        /// <summary>
        /// 统计打点
        /// </summary>
        void Report(string eventName, Dictionary<string,string> data);
    }
}