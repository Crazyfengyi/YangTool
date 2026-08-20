using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 文本自适应图片大小的脚本---使LayoutElement支持max width
/// </summary>
public class TextAdaptiveImage : MonoBehaviour
{
    private TextMeshProUGUI text; // 文本组件引用
    private LayoutElement layoutElement; // 布局元素组件引用
    public float maxWidth; // 最大宽度限制
    
    /// <summary>
    /// 注册文本值变化回调
    /// </summary>
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>(); // 获取文本组件
        layoutElement = GetComponent<LayoutElement>(); //获取布局元素组件
        text.RegisterDirtyVerticesCallback(OnTextValueChanged); // 注册文本值变化时的回调函数
    }

    /// <summary>
    /// 文本值变化时的回调函数,根据文本宽度调整布局元素的大小
    /// </summary>
    void OnTextValueChanged()
    {
        text.UnregisterDirtyVerticesCallback(OnTextValueChanged); //先取消回调注册，避免循环调用
        if (text.preferredWidth >= maxWidth) //如果文本宽度大于等于最大宽度
        {
            layoutElement.preferredWidth = maxWidth; // 设置布局元素宽度为最大宽度
        }
        else // 如果文本宽度小于最大宽度
        {
            layoutElement.preferredWidth = -1; // 设置为-1表示不限制宽度
        }
        text.RegisterDirtyVerticesCallback(OnTextValueChanged); // 重新注册回调函数
    }
}
