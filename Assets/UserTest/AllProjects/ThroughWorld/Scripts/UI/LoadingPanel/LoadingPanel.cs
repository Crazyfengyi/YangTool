/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-02
*/

using UnityEngine;
using YangTools.UGUI;

/// <summary>
/// 加载界面
/// </summary>
public class LoadingPanel : UGUIPanelBase<DefaultUGUIDataBase>
{
    public GameObject loading;

    public void OpenLoading()
    {
        loading.SetActive(true);
    }

    public void CloseLoading()
    {
        loading.SetActive(false);
    }
}