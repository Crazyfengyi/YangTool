/*
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2023.2.0b16 
 *创建时间:         2023-12-09 
*/
using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/// <summary>
/// 按颜色列表循环为 TMP 文本的每个字符添加颜色标签
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TextColor : MonoBehaviour
{
    // 需要添加颜色标签的 TMP 文本组件
    private TMP_Text textMesh;
    // 按字符循环使用的颜色列表
    public List<Color> colors;

    /// <summary>
    /// 取得 TMP 组件 开启富文本并处理初始文字
    /// </summary>
    public void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        textMesh.richText = true;
        SetText(textMesh.text);
    }

    /// <summary>
    /// 按字符循环包装颜色标签并替换当前文本 颜色列表须非空
    /// </summary>
    /// <param name="str">待处理的原始文本</param>
    public void SetText(string str)
    {
        string[] colorArray = colors.Select(ColorUtility.ToHtmlStringRGB).ToArray();
        
        StringBuilder stringBuilder = new StringBuilder();
        int index = 0;
        foreach (char c in str)
        {
            stringBuilder.Append($"<color=#{colorArray[index % colorArray.Length]}>{c}</color>");
            index++;
        }
        textMesh.text = stringBuilder.ToString();
    }
}