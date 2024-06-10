/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       WIN-VJ19D9AB7HB
 *UnityVersion：2023.2.0b16
 *创建时间:         2024-06-10
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;

public class PlayerInfoPanel : MonoBehaviour
{
    public Image bar;

    public RectTransform parent;
    public BuffShowItem buffShowItem;
    private List<BuffBase> allBuff = new List<BuffBase>();
    private List<BuffShowItem> allBuffShowItem = new List<BuffShowItem>();
    private void Start()
    {
        PlayerController target = GameActorManager.Instance.MainPlayer;
        target.roleBuffControl.OnBuffAddCallBack += Add;
        target.roleBuffControl.OnBuffRemoveCallBack += Remove;
    }

    public void Add(BuffBase buff)
    {
        allBuff.Add(buff);
        BuffShowItem item = GameObject.Instantiate(buffShowItem,parent,false);
        item.SetData(buff);
        allBuffShowItem.Add(item);
    }
    
    public void Remove(BuffBase buff)
    {
        int index = allBuff.IndexOf(buff);
        allBuff.Remove(buff);
        BuffShowItem target = allBuffShowItem[index];
        allBuffShowItem.Remove(target);
        Destroy(target.gameObject);
    }
}