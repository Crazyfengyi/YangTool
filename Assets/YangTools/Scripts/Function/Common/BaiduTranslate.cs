using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using LitJson;

namespace YangTools.Translate
{
    /// <summary>
    /// 百度翻译
    /// </summary>
    public class BaiduTranslate
    {
        private string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
        /// <summary>
        /// AppID
        /// </summary>
        public string appID;
        /// <summary>
        /// 密钥
        /// </summary>
        public string secretkey;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="appID">appID</param>
        /// <param name="secretkey">secretkey</param>
        public BaiduTranslate(string appID, string secretkey)
        {
            this.appID = appID;
            this.secretkey = secretkey;
        }
        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="q">原文</param>
        /// <param name="from">原语言</param>
        /// <param name="to">目标语言</param>
        /// <returns>返回翻译结果</returns>
        public string Translate(string q, LanguageType from, LanguageType to)
        {
            System.Random rd = new System.Random();
            string salt = rd.Next(100000).ToString();
            string sign = EncryptString(appID + q + salt + secretkey);
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + q;
            //url += "q=" + HttpUtility.UrlEncode(q);
            url += "&from=" + from.ToString();
            url += "&to=" + to.ToString();
            url += "&appid=" + appID;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            TransObj obj = JsonMapper.ToObject<TransObj>(retString);
            if (obj == null || obj.trans_result == null || (obj.trans_result.Count <= 0))
            {
                Debug.LogWarning("翻译失败了:" + q);
                return q;
            }
            return obj.trans_result[0].dst;
        }
    }
}

[Serializable]
public class TransObj
{
    /// <summary>
    /// 原语言
    /// </summary>
    public string from;
    /// <summary>
    /// 目标语言
    /// </summary>
    public string to;
    /// <summary>
    /// 翻译结果
    /// </summary>
    public List<TransResult> trans_result;
}

/// <summary>
/// 翻译结果
/// </summary>
[Serializable]
public class TransResult
{
    /// <summary>
    /// 原文
    /// </summary>
    public string src;
    /// <summary>
    /// 译文
    /// </summary>
    public string dst;
}

/// <summary>
/// 语言类型
/// </summary>
public enum LanguageType
{
    /// <summary>
    /// 自动检测
    /// </summary>
    Auto,
    /// <summary>
    /// 中文
    /// </summary>
    Zh,
    /// <summary>
    /// 英语
    /// </summary>
    En,
    /// <summary>
    /// 粤语
    /// </summary>
    Yue,
    /// <summary>
    /// 文言文
    /// </summary>
    Wyw,
    /// <summary>
    /// 日语
    /// </summary>
    Jp,
    /// <summary>
    /// 韩语
    /// </summary>
    Kor,
    /// <summary>
    /// 法语
    /// </summary>
    Fra,
    /// <summary>
    /// 西班牙语
    /// </summary>
    Spa,
    /// <summary>
    /// 泰语
    /// </summary>
    Th,
    /// <summary>
    /// 阿拉伯语
    /// </summary>
    Ara,
    /// <summary>
    /// 俄语
    /// </summary>
    Ru,
    /// <summary>
    /// 葡萄牙语
    /// </summary>
    Pt,
    /// <summary>
    /// 德语
    /// </summary>
    De,
    /// <summary>
    /// 意大利语
    /// </summary>
    It,
    /// <summary>
    /// 希腊语
    /// </summary>
    El,
    /// <summary>
    /// 荷兰语
    /// </summary>
    Nl,
    /// <summary>
    /// 波兰语
    /// </summary>
    Pl,
    /// <summary>
    /// 保加利亚语
    /// </summary>
    Bul,
    /// <summary>
    /// 爱沙尼亚语
    /// </summary>
    Est,
    /// <summary>
    /// 丹麦语
    /// </summary>
    Dan,
    /// <summary>
    /// 芬兰语
    /// </summary>
    Fin,
    /// <summary>
    /// 捷克语
    /// </summary>
    Cs,
    /// <summary>
    /// 罗马尼亚语
    /// </summary>
    Rom,
    /// <summary>
    /// 斯洛文尼亚语
    /// </summary>
    Slo,
    /// <summary>
    /// 瑞典语
    /// </summary>
    Swe,
    /// <summary>
    /// 匈牙利语
    /// </summary>
    HHu,
    /// <summary>
    /// 简体中文
    /// </summary>
    Cht,
    /// <summary>
    /// 越南语
    /// </summary>
    Vie
}