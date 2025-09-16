using System;
using System.Collections.Generic;
using System.Reflection;
using cfg.language;
using UnityEngine;
using YangTools.Scripts.Core;

public class LanguageManager : MonoSingleton<LanguageManager>
{
    public cfg.language.LanguageType languageType;
    
    private Dictionary<string, PropertyInfo> languageDic = new ();
    private TBLanguage tableData;

    public void Start()
    {
        tableData = GameTableManager.Instance.Tables.TBLanguage;
        Type type = typeof(Language);
        //获取所有实例属性
        PropertyInfo[] allFields = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (PropertyInfo field in allFields)
        {
            languageDic.Add(field.Name, field);
        }
        
        Debug.LogError($"测试:{GetLanguage("textStrId_1")}");
    }

    public string GetLanguage(string key)
    {
        // switch (languageType)
        // {
        //     case cfg.language.LanguageType.Chinese:
        //         return GameTableManager.Instance.Tables.TBLanguage.GetOrDefault(key).Chinese;
        //     case cfg.language.LanguageType.English:
        //         return GameTableManager.Instance.Tables.TBLanguage.GetOrDefault(key).English;
        //     default:
        //         return GameTableManager.Instance.Tables.TBLanguage.GetOrDefault(key).Chinese;
        // }
        
        //考虑反射获取字段或其它自动化高的配表方案
        string typeStr = languageType.ToString();
        if (languageDic.TryGetValue(typeStr,out PropertyInfo fieldInfo))
        {
            object temp = fieldInfo.GetValue(tableData.GetOrDefault(key));
            return temp.ToString();
        }
        return $"{key}:未找到多语言值";
    } 
}
