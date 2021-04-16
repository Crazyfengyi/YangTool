using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Threading;

public class OneKeySearchDuplicateFiles : SerializedScriptableObject
{
    private bool IsToggled;
    private int maxCount;
    private IEnumerator<FileInfo> fileInfoIEnumerator;

    [PropertySpace(10)]
    [Title("��Ҫ�������ļ���", "Ĭ��ΪAssetȫĿ¼", titleAlignment: TitleAlignments.Split)]
    [FolderPath(ParentFolder = "Assets", RequireExistingPath = true, AbsolutePath = true)]
    [LabelText("ѡ����Ҫ�������ļ���")]
    public string targetSearchFolder;

    [ShowInInspector]
    [DictionaryDrawerSettings(KeyLabel = "MD5ֵ", ValueLabel = "�ļ������б�")]
    private Dictionary<string, List<string>> sameMD5Group = new Dictionary<string, List<string>>();

    [ShowInInspector]
    [DictionaryDrawerSettings(KeyLabel = "�ļ�����", ValueLabel = "����·���б�")]
    private Dictionary<string, List<string>> sameNameGroup = new Dictionary<string, List<string>>();

    [ShowInInspector]
    [TitleGroup("�ظ��ļ��б�")]
    [HorizontalGroup("�ظ��ļ��б�/�ظ��ļ�")]
    [BoxGroup("�ظ��ļ��б�/�ظ��ļ�/MD5ֵ��ͬ", CenterLabel = true)]
    [PropertyOrder(1000)]
    [InfoBox("������ͬMD5ֵ�ļ�.", InfoMessageType.Error, "CheckSameMD5ResultGroup")]
    [ShowIf("$CheckSameMD5ResultGroup")]
    [DictionaryDrawerSettings(KeyLabel = "MD5ֵ", ValueLabel = "��ͬMD5ֵ�ļ�����")]
    private Dictionary<string, List<string>> sameMD5Result5Group = new Dictionary<string, List<string>>();


    [BoxGroup("�ظ��ļ��б�/�ظ��ļ�/����ֵ��ͬ", CenterLabel = true)]
    [ShowInInspector]
    [PropertyOrder(1000)]
    [InfoBox("������ͬ�����ļ�.", InfoMessageType.Error, "CheckSameNameResultGroup")]
    [ShowIf("$CheckSameNameResultGroup")]
    [DictionaryDrawerSettings(KeyLabel = "��ͬ�ļ�����", ValueLabel = "��Ӧ����·���б�")]
    private Dictionary<string, List<string>> sameNameResultGroup = new Dictionary<string, List<string>>();

    public bool CheckSameMD5ResultGroup()
    {
        return sameMD5Result5Group.Count > 0;
    }

    private bool CheckSameNameResultGroup()
    {
        return sameNameResultGroup.Count > 0;
    }

    [PropertySpace(10, 20)]
    [ShowIf("@ IsToggled== false")]
    [Button("��ʼ����", ButtonSizes.Large)]
    public void StartSearch()
    {
        if (string.IsNullOrEmpty(targetSearchFolder))
        {
            targetSearchFolder = Application.dataPath;
        }
        ResetData();
        DirectoryInfo directoryInfo = new DirectoryInfo(targetSearchFolder);
        var filesGroup = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(x => x.Extension != ".meta");

        maxCount = filesGroup.Count();
        fileInfoIEnumerator = filesGroup.GetEnumerator();
        IsToggled = true;
        EditorApplication.update += Updte;
    }

    /// <summary>
    /// ��������
    /// </summary>
    private void ResetData()
    {
        maxCount = 0;
        MaxCount = 0;
        sameMD5Group.Clear();
        sameNameGroup.Clear();
        sameMD5Result5Group.Clear();
        sameNameResultGroup.Clear();
        fileInfoIEnumerator = null;
    }

    /// <summary>
    /// ���˵�û���ظ��ļ�������
    /// </summary>
    private void FilterDictionary()
    {
        sameMD5Result5Group = sameMD5Group.Where(x => x.Value.Count > 1).ToDictionary(p => p.Key, p => p.Value);
        sameNameResultGroup = sameNameGroup.Where(x => x.Value.Count > 1).ToDictionary(p => p.Key, p => p.Value);
    }

    [ReadOnly]
    [ProgressBar(0, "maxCount", DrawValueLabel = true, ValueLabelAlignment = TextAlignment.Left, ColorMember = "GetHealthBarColor", Height = 30)]
    [ShowInInspector]
    [HideLabel]
    [ShowIf("@ IsToggled== true")]
    public int MaxCount { get; set; }//���ƽ�����

    private Color GetHealthBarColor(int value)
    {
        maxCount = maxCount == 0 ? 1 : maxCount;
        return Color.Lerp(Color.red, Color.green, Mathf.Pow((float)value / maxCount, 2));
    }

    public void Updte()
    {
        if (IsToggled)
        {
            if (fileInfoIEnumerator.MoveNext())
            {
                //��ȡ��ӦHashֵ
                string hashValue = GetMD5HashFromFile(fileInfoIEnumerator.Current.FullName);
                if (!sameMD5Group.ContainsKey(hashValue))
                {
                    sameMD5Group[hashValue] = new List<string>();
                }
                sameMD5Group[hashValue].Add("����Ϊ��" + fileInfoIEnumerator.Current.Name);

                //��ȡ����
                string fileName = fileInfoIEnumerator.Current.Name;

                if (!sameNameGroup.ContainsKey(fileName))
                {
                    sameNameGroup[fileName] = new List<string>();
                }
                sameNameGroup[fileName].Add("·��Ϊ��" + fileInfoIEnumerator.Current.FullName);

                ++MaxCount;
            }
            else
            {
                EditorApplication.update -= Updte;
                IsToggled = false;
                FilterDictionary();
                Debug.Log("<color=green>ע��</color>");
            }
        }
    }

    /// <summary>
    /// �����ļ�MD5ֵ
    /// </summary>
    /// <param name="fileFullName"></param>
    /// <returns></returns>
    public string GetMD5HashFromFile(string fileFullName)
    {
        try
        {
            FileStream file = new FileStream(fileFullName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            return BitConverter.ToString(retVal).ToLower().Replace("-", "");
        }
        catch
        {
            throw;
        }
    }
}