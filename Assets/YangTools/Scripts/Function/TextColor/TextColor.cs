/** 
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

[RequireComponent(typeof(TMP_Text))]
public class TextColor : MonoBehaviour
{
    private TMP_Text textMesh;
    public List<Color> colors;

    public void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        textMesh.richText = true;
        SetText(textMesh.text);
    }

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