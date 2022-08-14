/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-08-13 
*/
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
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
    [NonSerialized]
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
    [Button("测试")]
    public void Test()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
    #endregion
}