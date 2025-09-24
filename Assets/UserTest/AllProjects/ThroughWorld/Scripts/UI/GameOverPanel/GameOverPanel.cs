/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2022-12-26
*/

using UnityEngine;
using UnityEngine.UI;
using YangTools;
using YangTools.Scripts.Core.YangUGUI;

public class GameOverPanel : UGUIPanelBase<UGUIDataBase>
{
    [SerializeField]
    private Button btn;

    private void Awake()
    {
        btn.onClick.AddListener(Btn_OnClick);
    }

    /// <summary>
    /// 界面打开
    /// </summary>
    public override void OnOpen(object userData)
    {
        base.OnOpen(userData);
    }

    public void Btn_OnClick()
    {
        GameSceneManager.Instance.Load("GameMain");
        CloseSelfPanel();
        GameActorManager.Instance.CreateMainPlayer();
    }
}