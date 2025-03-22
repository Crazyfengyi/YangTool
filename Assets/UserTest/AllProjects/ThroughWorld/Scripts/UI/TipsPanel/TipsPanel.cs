/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-02
*/

using DG.Tweening;
using TMPro;
using UnityEngine;
using YangTools.Scripts.Core.YangUGUI;

/// <summary>
/// 提示界面
/// </summary>
public class TipsPanel : UGUIPanelBase<DefaultUGUIDataBase>
{
    public GameObject prefab;

    private static int num = 0;

    public void ShowTip(string str)
    {
        num++;
        GameObject temp = Instantiate(prefab, transform);
        temp.GetComponentInChildren<TMP_Text>().text = str;

        DOTween.Sequence()
            .AppendInterval(num * 0.5f)
            .AppendCallback(() =>
            {
                temp.SetActive(true);
            })
            .Append(temp.transform.DOLocalMoveY(temp.transform.localPosition.y + 230, 0.6f))
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                num--;
                Destroy(temp);
            });
    }
}