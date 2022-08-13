using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// BUFF配置信息
/// </summary>
public class BuffConfig : ConfigBase
{
    /// <summary>
    /// 名称
    /// </summary>
    public string ConfigName
    {
        get
        {
            return buffID.ToString();
        }
    }
    [BoxGroup("基本信息"), LabelText("BuffID")]
    public BuffID buffID;
    [BoxGroup("基本信息"), LabelText("描述"), TextArea()]
    public string des;
    [BoxGroup("基本信息"), LabelText("图标"), PreviewField()]
    public Texture2D icon;
    [BoxGroup("基本信息"), LabelText("类型")]
    public BuffType buffType;
    [BoxGroup("基本信息"), LabelText("CD")]
    public int CD;
    [BoxGroup("基本信息"), LabelText("结束时机")]
    public BuffEndType buffEndType;

    [BoxGroup("特效信息"), LabelText("特效")]
    public EffectInfo effect;

}

[CreateAssetMenu(menuName = "RoleData/BuffConfigSave", fileName = "BuffConfigSave")]
public class BuffConfigSave : SaveDataWithSO<BuffConfig>
{
    #region 编辑器工具
#if UNITY_EDITOR
    [Button("根据枚举设置ID")]
    public void AutoSetId()
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            BuffConfig item = dataList[i];
            item.id = item.buffID.GetHashCode();
        }
    }
    [Button("检查ID是否有重复的")]
    public void CheckIdIsSame()
    {
        //所有id
        HashSet<int> temp = new HashSet<int>();
        for (int i = 0; i < dataList.Count; i++)
        {
            if (temp.Contains(dataList[i].id))
            {
                EditorUtility.DisplayDialog("检查结果", $"检查到第{i}个为重复的数据---重复的ID为:{dataList[i].id}", "OK");
                return;
            }
            temp.Add(dataList[i].id);
        }

        EditorUtility.DisplayDialog("检查结果", $"没有重复", "OK");
    }
    [LabelText("枚举名称")]
    public string newEnumType = "";
    [Button("添加BUFF枚举")]
    public void AddEnum()
    {
        string path = "Assets/UserTest/AllProjects/ThroughWorld/Scripts/Actor/BUFF/BuffDataStruct.cs";
        TextAsset temp = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        string str = temp.text;
        string[] splitStr = new string[] { "public enum BuffID" };
        string[] strEnd = str.Split(splitStr, StringSplitOptions.None);

        if (strEnd[1].Contains(newEnumType))
        {
            EditorUtility.DisplayDialog("提示", $"有重复枚举,添加失败", "OK");
            return;
        }
        string temp1 = strEnd[1].TrimEnd('}');
        temp1 += $"    {newEnumType},";
        temp1 += "\n}";
        string result = strEnd[0] + "public enum BuffID" + temp1;

        FileStream file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
        StreamWriter sr = new StreamWriter(file);
        sr.Write(result);
        sr.Dispose();

        EditorUtility.SetDirty(temp);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
    #endregion
}