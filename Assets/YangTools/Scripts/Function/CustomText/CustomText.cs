/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-04-02 
*/
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine;
using System;

namespace YangTools
{
    /// <summary>
    /// 自定义文本
    /// </summary>
    public class CustomText : TextMeshProUGUI
    {
        private List<GameObject> rubyList = new List<GameObject>();//注音列表
        private int typingIndex;//打字进度下标
        private float defaultInterval = 0.02f;//默认间隔
        private Action endCallBack;//结束回调
        private CustomTextPreprocessor SelfPreprocessor => (CustomTextPreprocessor)textPreprocessor;
        public CustomText()
        {
            textPreprocessor = new CustomTextPreprocessor();
        }
        /// <summary>
        /// 设置注音
        /// </summary>
        /// <param name="data"></param>
        private void SetRubyText(RubyData data)
        {
            GameObject obj = Resources.Load<GameObject>("RubyText");
            GameObject ruby = Instantiate(obj, transform);
            ruby.GetComponent<TextMeshProUGUI>().SetText(data.RubyContent);
            ruby.GetComponent<TextMeshProUGUI>().color = textInfo.characterInfo[data.StartIndex].color;

            bool partHaveSpecial = false;//是否有特殊字符
            for (int i = data.StartIndex; i <= data.EndIndex; i++)
            {
                if (textInfo.characterInfo[i].character == '\n' || textInfo.characterInfo[i].character == ' ')
                {
                    partHaveSpecial = true;
                    break;
                }
            }
            //有换行
            if (m_lineNumber > 0)
            {
                partHaveSpecial = true;
            }

            //有特殊字符--需要更换规则
            if (partHaveSpecial)
            {
                //改为取中间字符的头顶位置
                int mid = Mathf.FloorToInt((data.StartIndex + data.EndIndex) / 2f);
                do
                {
                    ruby.transform.localPosition = new Vector3((textInfo.characterInfo[mid].topLeft.x + textInfo.characterInfo[mid].topRight.x) / 2, textInfo.characterInfo[mid].topLeft.y, 0);

                } while ((textInfo.characterInfo[mid].character == '\n' | textInfo.characterInfo[mid].character == ' ') & --mid >= data.StartIndex);//只能用&不能用&&,需要--mid
            }
            else
            {
                //取开始结束字符的中间位置
                ruby.transform.localPosition = (textInfo.characterInfo[data.StartIndex].topLeft + textInfo.characterInfo[data.EndIndex].topRight) / 2;
            }

            rubyList.Add(ruby);
        }
        /// <summary>
        /// 清空注音
        /// </summary>
        private void ClearRuby()
        {
            for (int i = 0; i < rubyList.Count; i++)
            {
                DestroyImmediate(rubyList[i]);
            }
            rubyList.Clear();
        }
        /// <summary>
        /// 打字效果显示文字
        /// </summary>
        public void ShowTextByTyping(string content, Action _endCallBack = null)
        {
            ClearRuby();
            endCallBack = _endCallBack;
            SetText(content);
            StartCoroutine(Typing());
        }
        /// <summary>
        /// 打字效果
        /// </summary>
        IEnumerator Typing()
        {
            //强制网格更新
            ForceMeshUpdate();
            //所有字透明度先设为125
            for (int i = 0; i < m_characterCount; i++)
            {
                SetSingleCharaterAlpha(i, 0);
            }
            typingIndex = 0;
            yield return null;
            while (typingIndex < m_characterCount)
            {
                StartCoroutine(FadeInCharacter(typingIndex));
                if (SelfPreprocessor.intervalDic.TryGetValue(typingIndex, out float result))
                {
                    yield return new WaitForSecondsRealtime(result);
                }
                else
                {
                    yield return new WaitForSecondsRealtime(defaultInterval);
                }
                typingIndex++;
            }

            endCallBack?.Invoke();
        }
        /// <summary>
        /// 渐显
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="duration">时间</param>
        IEnumerator FadeInCharacter(int index, float duration = 0.2f)
        {
            if (SelfPreprocessor.TryGetRubyStartFrom(index, out RubyData data))
            {
                SetRubyText(data);
            }

            if (duration <= 0)
            {
                SetSingleCharaterAlpha(index, 255);
            }
            else
            {
                float timer = 0;
                while (timer < duration)
                {
                    timer = Mathf.Min(duration, timer + Time.unscaledDeltaTime);
                    SetSingleCharaterAlpha(index, (byte)(255 * timer / duration));
                    yield return null;
                }
            }
        }
        /// <summary>
        /// 改变单个文字的Alpha
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="newAlpha">0~255</param>
        private void SetSingleCharaterAlpha(int index, byte newAlpha)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[index];//文字信息
            //不可见字符(空格是不可见字符)
            if (!charInfo.isVisible)
            {
                return;
            }

            int matIndex = charInfo.materialReferenceIndex;//材质下标
            int vertIndex = charInfo.vertexIndex;//顶点下标
            for (int i = 0; i < 4; i++)
            {
                textInfo.meshInfo[matIndex].colors32[vertIndex + i].a = newAlpha;
            }
            //更新顶点数据
            UpdateVertexData();
        }
    }
    /// <summary>
    /// 预处理器
    /// </summary>
    public class CustomTextPreprocessor : ITextPreprocessor
    {
        //间隔字典(第几个下标,间隔时间)
        public Dictionary<int, float> intervalDic = new Dictionary<int, float>();
        public List<RubyData> rubyList = new List<RubyData>();
        /// <summary>
        /// 获得注音
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        public bool TryGetRubyStartFrom(int index, out RubyData data)
        {
            data = new RubyData(0, "");
            foreach (var item in rubyList)
            {
                if (item.StartIndex == index)
                {
                    data = item;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 预处理文本
        /// </summary>
        /// <param name="text">文本</param>
        public string PreprocessText(string text)
        {
            intervalDic.Clear();
            rubyList.Clear();

            if (string.IsNullOrEmpty(text)) return "";

            //处理字符串
            string processingText = text;
            //匹配最近的<>括号
            string pattern = "<.*?>";
            Match match = Regex.Match(processingText, pattern);
            while (match.Success)
            {
                //事例:<0.2>取括号里的内容
                string label = match.Value.Substring(1, match.Length - 2);
                if (float.TryParse(label, out float result))
                {
                    intervalDic[match.Index - 1] = result;
                }
                else if (Regex.IsMatch(label, "^r=.*"))
                {
                    rubyList.Add(new RubyData(match.Index, label.Substring(2)));
                }
                else if (label == "/r")
                {
                    if (rubyList.Count > 0)
                    {
                        rubyList[rubyList.Count - 1].EndIndex = match.Index - 1;
                    }
                }

                //如果是图片文字就删除并用*占位
                if (Regex.IsMatch(label, "^sprite=.*"))
                {
                    processingText = processingText.Remove(match.Index, match.Length);
                    processingText = processingText.Insert(match.Index, "*");
                }
                else
                {
                    //删掉<>和里面的内容
                    processingText = processingText.Remove(match.Index, match.Length);
                }
                //再次匹配
                match = Regex.Match(processingText, pattern);
            }
            //还原
            processingText = text;
            //替换
            pattern = @"(<(\d+)(\.\d+)?>)|(</r>)|(<r=.*?>)";
            processingText = Regex.Replace(processingText, pattern, "");

            return processingText;
        }
    }
    /// <summary>
    /// 注音数据
    /// </summary>
    public class RubyData
    {
        public int StartIndex { get; }
        public int EndIndex { get; set; }
        public string RubyContent { get; set; }
        public RubyData(int startIndex, string content)
        {
            StartIndex = startIndex;
            RubyContent = content;
            EndIndex = StartIndex;
        }
    }
}