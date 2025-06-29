/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-11-13
 */

using System;
using System.Linq;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core.YangMiniMap;
using YangTools.Scripts.Core;

namespace YangTools
{
    [AddComponentMenu("游戏管理器")]
    public class GameManager : MonoSingleton<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();
            LoadOnProgress onProgress = new LoadOnProgress();
            onProgress.OnStartLoad += GameProjectileManager.Instance.OnSceneChangeStart;
            onProgress.OnStartLoad += GameUIManager.Instance.OnSceneChangeStart;
            onProgress.OnStartLoad += GameActorManager.Instance.OnSceneChangeStart;
            GameSceneManager.Instance.SetOnProgress(onProgress);
        }

        public void Update()
        {
            //TODO:UI弹窗打开时显示鼠标指针--指针隐藏不会触发按钮抬起
            //if(UIMonoInstance.Instance.ActiveUIPanel.s)

            //if (Input.GetKeyDown(KeyCode.Mouse0))
            //{
            //    if (GameActorManager.Instance.MainPlayer)
            //    {
            //        YangToolsManager.SetCursorLock(true);
            //    }
            //}

            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    YangToolsManager.SetCursorLock(false);
            //}

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //string[] name = { "x","y"};
                //var temp = name.Skip(1).Take(1);
                //var a = name.AsSpan();

                //屏幕坐标
                Vector3 tempPos = Input.mousePosition;
                tempPos.z = 10;
                Vector3 WorldPos = GameUIManager.Instance.UICamera.ScreenToWorldPoint(tempPos);
                GameEffectManager.Instance.PlayEffect("ClickEffect", worldPos: WorldPos);
            }
        }

        #region 场景加载

        /// <summary>
        /// 场景加载
        /// </summary>
        public void SceneLoad(string sceneName)
        {
            GameSceneManager.Instance.IsAutoSkip = true;
            UICommonTool.Instance.SetSeceneLoading(true);

            MiniMapManager.Instance.SetMiniMapShow(false);

            GameSceneManager.Instance.OnProgressEvent.OnEndLoad = (progrese) =>
            {
                UICommonTool.Instance.SetSeceneLoading(false);
                MiniMapManager.Instance.SetMiniMapShow(true);
            };
            GameSceneManager.Instance.OnProgressEvent.OnLoading = (progrese) =>
            {
                //TODO:
            };
            GameSceneManager.Instance.Load(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        #endregion
    }
}