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
    /// �ٶȷ���
    /// </summary>
    public class BaiduTranslate
    {
        private string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // ���ַ���ת�����ֽ�����
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // ���ü��ܷ���
            byte[] byteNew = md5.ComputeHash(byteOld);
            // �����ܽ��ת��Ϊ�ַ���
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // ���ֽ�ת����16���Ʊ�ʾ���ַ�����
                sb.Append(b.ToString("x2"));
            }
            // ���ؼ��ܵ��ַ���
            return sb.ToString();
        }
        /// <summary>
        /// AppID
        /// </summary>
        public string appID;
        /// <summary>
        /// ��Կ
        /// </summary>
        public string secretkey;
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="appID">appID</param>
        /// <param name="secretkey">secretkey</param>
        public BaiduTranslate(string appID, string secretkey)
        {
            this.appID = appID;
            this.secretkey = secretkey;
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="q">ԭ��</param>
        /// <param name="from">ԭ����</param>
        /// <param name="to">Ŀ������</param>
        /// <returns>���ط�����</returns>
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
                Debug.LogWarning("����ʧ����:" + q);
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
    /// ԭ����
    /// </summary>
    public string from;
    /// <summary>
    /// Ŀ������
    /// </summary>
    public string to;
    /// <summary>
    /// ������
    /// </summary>
    public List<TransResult> trans_result;
}

/// <summary>
/// ������
/// </summary>
[Serializable]
public class TransResult
{
    /// <summary>
    /// ԭ��
    /// </summary>
    public string src;
    /// <summary>
    /// ����
    /// </summary>
    public string dst;
}

/// <summary>
/// ��������
/// </summary>
public enum LanguageType
{
    /// <summary>
    /// �Զ����
    /// </summary>
    Auto,
    /// <summary>
    /// ����
    /// </summary>
    Zh,
    /// <summary>
    /// Ӣ��
    /// </summary>
    En,
    /// <summary>
    /// ����
    /// </summary>
    Yue,
    /// <summary>
    /// ������
    /// </summary>
    Wyw,
    /// <summary>
    /// ����
    /// </summary>
    Jp,
    /// <summary>
    /// ����
    /// </summary>
    Kor,
    /// <summary>
    /// ����
    /// </summary>
    Fra,
    /// <summary>
    /// ��������
    /// </summary>
    Spa,
    /// <summary>
    /// ̩��
    /// </summary>
    Th,
    /// <summary>
    /// ��������
    /// </summary>
    Ara,
    /// <summary>
    /// ����
    /// </summary>
    Ru,
    /// <summary>
    /// ��������
    /// </summary>
    Pt,
    /// <summary>
    /// ����
    /// </summary>
    De,
    /// <summary>
    /// �������
    /// </summary>
    It,
    /// <summary>
    /// ϣ����
    /// </summary>
    El,
    /// <summary>
    /// ������
    /// </summary>
    Nl,
    /// <summary>
    /// ������
    /// </summary>
    Pl,
    /// <summary>
    /// ����������
    /// </summary>
    Bul,
    /// <summary>
    /// ��ɳ������
    /// </summary>
    Est,
    /// <summary>
    /// ������
    /// </summary>
    Dan,
    /// <summary>
    /// ������
    /// </summary>
    Fin,
    /// <summary>
    /// �ݿ���
    /// </summary>
    Cs,
    /// <summary>
    /// ����������
    /// </summary>
    Rom,
    /// <summary>
    /// ˹����������
    /// </summary>
    Slo,
    /// <summary>
    /// �����
    /// </summary>
    Swe,
    /// <summary>
    /// ��������
    /// </summary>
    HHu,
    /// <summary>
    /// ��������
    /// </summary>
    Cht,
    /// <summary>
    /// Խ����
    /// </summary>
    Vie
}