/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-04-02
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// 支持注音和打字效果的自定义文本组件
    /// </summary>
    public class CustomText : TextMeshProUGUI
    {
        private const string RubyPrefabPath = "Ruby/RubyText"; //注音预制体路径
        private const float DefaultInterval = 0.02f; //默认打字间隔
        private const float FadeDuration = 0.2f; //单字渐显时长

        private readonly List<GameObject> rubyList = new List<GameObject>(); //已创建的注音对象
        private GameObject rubyPrefab; //注音预制体
        private Action endCallBack; //打字结束回调
        private int activeFadeCount; //正在执行的渐显协程数量

        private GameObject RubyPrefab
        {
            get
            {
                if (rubyPrefab == null)
                {
                    rubyPrefab = Resources.Load<GameObject>(RubyPrefabPath);
                }

                return rubyPrefab;
            }
        }

        private CustomTextPreprocessor SelfPreprocessor
        {
            get
            {
                if (textPreprocessor is CustomTextPreprocessor preprocessor)
                {
                    return preprocessor;
                }

                preprocessor = new CustomTextPreprocessor();
                textPreprocessor = preprocessor;
                return preprocessor;
            }
        }

        /// <summary>
        /// 初始化文本预处理器
        /// </summary>
        public CustomText()
        {
            textPreprocessor = new CustomTextPreprocessor();
        }

        /// <summary>
        /// 销毁组件时清理运行时注音对象
        /// </summary>
        private void OnDestroy()
        {
            ClearRuby();
        }

        #region 注音处理

        /// <summary>
        /// 创建指定范围的注音文本
        /// </summary>
        /// <param name="data">注音数据</param>
        private void SetRubyText(RubyData data)
        {
            if (!TryGetRubyCharacterRange(data, out int firstCharacterIndex, out int lastCharacterIndex))
            {
                return;
            }

            if (RubyPrefab == null)
            {
                Debug.LogWarning($"{nameof(CustomText)} 未找到注音预制体 {RubyPrefabPath}", this);
                return;
            }

            GameObject ruby = Instantiate(RubyPrefab, transform);
            if (!ruby.TryGetComponent(out TextMeshProUGUI rubyText))
            {
                Debug.LogWarning($"{nameof(CustomText)} 注音预制体缺少 {nameof(TextMeshProUGUI)} 组件", ruby);
                DestroyRubyObject(ruby);
                return;
            }

            rubyText.SetText(data.RubyContent ?? string.Empty);
            rubyText.color = textInfo.characterInfo[firstCharacterIndex].color;
            ruby.transform.localPosition = GetRubyPosition(firstCharacterIndex, lastCharacterIndex);
            rubyList.Add(ruby);
        }

        /// <summary>
        /// 获得注音覆盖范围内的首尾可见字符
        /// </summary>
        /// <param name="data">注音数据</param>
        /// <param name="firstCharacterIndex">首个可见字符下标</param>
        /// <param name="lastCharacterIndex">末个可见字符下标</param>
        /// <returns>是否找到有效字符</returns>
        private bool TryGetRubyCharacterRange(RubyData data, out int firstCharacterIndex, out int lastCharacterIndex)
        {
            firstCharacterIndex = -1;
            lastCharacterIndex = -1;
            if (data == null || data.StartIndex < 0 || data.EndIndex < data.StartIndex || data.EndIndex >= m_characterCount)
            {
                return false;
            }

            for (int i = data.StartIndex; i <= data.EndIndex; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                {
                    continue;
                }

                if (firstCharacterIndex < 0)
                {
                    firstCharacterIndex = i;
                }

                lastCharacterIndex = i;
            }

            return firstCharacterIndex >= 0;
        }

        /// <summary>
        /// 获得注音的本地坐标
        /// </summary>
        /// <param name="firstCharacterIndex">首个可见字符下标</param>
        /// <param name="lastCharacterIndex">末个可见字符下标</param>
        /// <returns>注音坐标</returns>
        private Vector3 GetRubyPosition(int firstCharacterIndex, int lastCharacterIndex)
        {
            TMP_CharacterInfo firstCharacter = textInfo.characterInfo[firstCharacterIndex];
            TMP_CharacterInfo lastCharacter = textInfo.characterInfo[lastCharacterIndex];
            if (firstCharacter.lineNumber == lastCharacter.lineNumber)
            {
                return (firstCharacter.topLeft + lastCharacter.topRight) * 0.5f;
            }

            int centerCharacterIndex = GetClosestVisibleCharacterIndex(firstCharacterIndex, lastCharacterIndex);
            TMP_CharacterInfo centerCharacter = textInfo.characterInfo[centerCharacterIndex];
            return (centerCharacter.topLeft + centerCharacter.topRight) * 0.5f;
        }

        /// <summary>
        /// 获得范围中最靠近中心的可见字符
        /// </summary>
        /// <param name="firstCharacterIndex">首个可见字符下标</param>
        /// <param name="lastCharacterIndex">末个可见字符下标</param>
        /// <returns>可见字符下标</returns>
        private int GetClosestVisibleCharacterIndex(int firstCharacterIndex, int lastCharacterIndex)
        {
            int centerCharacterIndex = (firstCharacterIndex + lastCharacterIndex) / 2;
            for (int offset = 0; offset <= lastCharacterIndex - firstCharacterIndex; offset++)
            {
                int rightCharacterIndex = centerCharacterIndex + offset;
                if (rightCharacterIndex <= lastCharacterIndex && textInfo.characterInfo[rightCharacterIndex].isVisible)
                {
                    return rightCharacterIndex;
                }

                int leftCharacterIndex = centerCharacterIndex - offset;
                if (leftCharacterIndex >= firstCharacterIndex && textInfo.characterInfo[leftCharacterIndex].isVisible)
                {
                    return leftCharacterIndex;
                }
            }

            return firstCharacterIndex;
        }

        /// <summary>
        /// 清理已创建的注音对象
        /// </summary>
        private void ClearRuby()
        {
            for (int i = 0; i < rubyList.Count; i++)
            {
                DestroyRubyObject(rubyList[i]);
            }

            rubyList.Clear();
        }

        /// <summary>
        /// 根据当前运行状态销毁注音对象
        /// </summary>
        /// <param name="ruby">注音对象</param>
        private void DestroyRubyObject(GameObject ruby)
        {
            if (ruby == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(ruby);
                return;
            }

            DestroyImmediate(ruby);
        }

        #endregion

        #region 打字效果

        /// <summary>
        /// 使用打字效果显示文字
        /// </summary>
        /// <param name="content">显示内容</param>
        /// <param name="callback">结束回调</param>
        public void ShowTextByTyping(string content, Action callback = null)
        {
            StopAllCoroutines();
            activeFadeCount = 0;
            ClearRuby();
            endCallBack = callback;
            SetText(content ?? string.Empty);
            StartCoroutine(Typing());
        }

        /// <summary>
        /// 逐字显示文本
        /// </summary>
        private IEnumerator Typing()
        {
            ForceMeshUpdate();
            SetAllCharactersAlpha(0);
            yield return null;

            for (int index = 0; index < m_characterCount; index++)
            {
                StartCoroutine(FadeInCharacter(index));
                if (SelfPreprocessor.IntervalDic.TryGetValue(index, out float interval))
                {
                    yield return new WaitForSecondsRealtime(interval);
                    continue;
                }

                yield return new WaitForSecondsRealtime(DefaultInterval);
            }

            while (activeFadeCount > 0)
            {
                yield return null;
            }

            Action callback = endCallBack;
            endCallBack = null;
            callback?.Invoke();
        }

        /// <summary>
        /// 渐显单个字符并创建对应注音
        /// </summary>
        /// <param name="index">字符下标</param>
        private IEnumerator FadeInCharacter(int index)
        {
            activeFadeCount++;
            try
            {
                if (SelfPreprocessor.TryGetRubyStartFrom(index, out RubyData data))
                {
                    SetRubyText(data);
                }

                if (index < 0 || index >= m_characterCount || !textInfo.characterInfo[index].isVisible)
                {
                    yield break;
                }

                float timer = 0f;
                while (timer < FadeDuration)
                {
                    timer = Mathf.Min(FadeDuration, timer + Time.unscaledDeltaTime);
                    if (SetSingleCharacterAlpha(index, (byte)(255f * timer / FadeDuration)))
                    {
                        UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                    }

                    yield return null;
                }
            }
            finally
            {
                activeFadeCount--;
            }
        }

        /// <summary>
        /// 设置所有字符的透明度
        /// </summary>
        /// <param name="alpha">目标透明度</param>
        private void SetAllCharactersAlpha(byte alpha)
        {
            bool hasChanged = false;
            for (int i = 0; i < m_characterCount; i++)
            {
                hasChanged |= SetSingleCharacterAlpha(i, alpha);
            }

            if (hasChanged)
            {
                UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }
        }

        /// <summary>
        /// 改变单个文字的透明度
        /// </summary>
        /// <param name="index">字符下标</param>
        /// <param name="newAlpha">目标透明度</param>
        /// <returns>是否修改顶点颜色</returns>
        private bool SetSingleCharacterAlpha(int index, byte newAlpha)
        {
            if (index < 0 || index >= m_characterCount)
            {
                return false;
            }

            TMP_CharacterInfo characterInfo = textInfo.characterInfo[index];
            if (!characterInfo.isVisible)
            {
                return false;
            }

            int materialIndex = characterInfo.materialReferenceIndex;
            int vertexIndex = characterInfo.vertexIndex;
            if (materialIndex < 0 || materialIndex >= textInfo.meshInfo.Length || vertexIndex < 0)
            {
                return false;
            }

            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
            if (vertexIndex + 3 >= colors.Length)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                colors[vertexIndex + i].a = newAlpha;
            }

            return true;
        }

        #endregion
    }

    /// <summary>
    /// 解析打字间隔和注音标签的文本预处理器
    /// </summary>
    public class CustomTextPreprocessor : ITextPreprocessor
    {
        public readonly Dictionary<int, float> IntervalDic = new Dictionary<int, float>(); //字符间隔字典
        public readonly List<RubyData> RubyList = new List<RubyData>(); //注音数据列表

        private readonly Stack<RubyData> rubyStack = new Stack<RubyData>(); //未闭合的注音标签

        /// <summary>
        /// 获得从指定字符开始的注音数据
        /// </summary>
        /// <param name="index">字符下标</param>
        /// <param name="data">注音数据</param>
        /// <returns>是否存在注音数据</returns>
        public bool TryGetRubyStartFrom(int index, out RubyData data)
        {
            for (int i = 0; i < RubyList.Count; i++)
            {
                if (RubyList[i].StartIndex == index)
                {
                    data = RubyList[i];
                    return true;
                }
            }

            data = null;
            return false;
        }

        /// <summary>
        /// 预处理文本中的打字间隔和注音标签
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <returns>供 TextMeshPro 显示的文本</returns>
        public string PreprocessText(string text)
        {
            IntervalDic.Clear();
            RubyList.Clear();
            rubyStack.Clear();
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(text.Length);
            int visibleCharacterIndex = 0;
            int contentStartIndex = 0;
            int searchStartIndex = 0;
            while (searchStartIndex < text.Length)
            {
                int tagStartIndex = text.IndexOf('<', searchStartIndex);
                if (tagStartIndex < 0)
                {
                    break;
                }

                int tagEndIndex = text.IndexOf('>', tagStartIndex + 1);
                if (tagEndIndex < 0)
                {
                    break;
                }

                int plainTextLength = tagStartIndex - contentStartIndex;
                builder.Append(text, contentStartIndex, plainTextLength);
                visibleCharacterIndex += plainTextLength;

                string label = text.Substring(tagStartIndex + 1, tagEndIndex - tagStartIndex - 1);
                if (TryParseInterval(label, out float interval))
                {
                    if (visibleCharacterIndex > 0)
                    {
                        IntervalDic[visibleCharacterIndex - 1] = interval;
                    }
                }
                else if (label.StartsWith("r=", StringComparison.Ordinal))
                {
                    rubyStack.Push(new RubyData(visibleCharacterIndex, label.Substring(2)));
                }
                else if (label == "/r")
                {
                    CloseRuby(visibleCharacterIndex);
                }
                else
                {
                    builder.Append(text, tagStartIndex, tagEndIndex - tagStartIndex + 1);
                    if (IsSpriteTag(label))
                    {
                        visibleCharacterIndex++;
                    }
                }

                contentStartIndex = tagEndIndex + 1;
                searchStartIndex = contentStartIndex;
            }

            builder.Append(text, contentStartIndex, text.Length - contentStartIndex);
            return builder.ToString();
        }

        /// <summary>
        /// 解析打字间隔标签
        /// </summary>
        /// <param name="label">标签内容</param>
        /// <param name="interval">打字间隔</param>
        /// <returns>是否为有效间隔标签</returns>
        private bool TryParseInterval(string label, out float interval)
        {
            return float.TryParse(label, NumberStyles.Float, CultureInfo.InvariantCulture, out interval) && interval >= 0f;
        }

        /// <summary>
        /// 关闭最近的注音标签
        /// </summary>
        /// <param name="visibleCharacterIndex">当前字符下标</param>
        private void CloseRuby(int visibleCharacterIndex)
        {
            if (rubyStack.Count == 0)
            {
                return;
            }

            RubyData data = rubyStack.Pop();
            data.EndIndex = visibleCharacterIndex - 1;
            if (data.EndIndex >= data.StartIndex)
            {
                RubyList.Add(data);
            }
        }

        /// <summary>
        /// 判断标签是否表示 TextMeshPro 精灵
        /// </summary>
        /// <param name="label">标签内容</param>
        /// <returns>是否为精灵标签</returns>
        private bool IsSpriteTag(string label)
        {
            return label == "sprite" || label.StartsWith("sprite=", StringComparison.Ordinal) || label.StartsWith("sprite ", StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 注音范围和内容数据
    /// </summary>
    public class RubyData
    {
        public int StartIndex { get; } //起始字符下标
        public int EndIndex { get; set; } //结束字符下标
        public string RubyContent { get; set; } //注音内容

        /// <summary>
        /// 创建注音数据
        /// </summary>
        /// <param name="startIndex">起始字符下标</param>
        /// <param name="content">注音内容</param>
        public RubyData(int startIndex, string content)
        {
            StartIndex = startIndex;
            EndIndex = startIndex;
            RubyContent = content;
        }
    }
}
