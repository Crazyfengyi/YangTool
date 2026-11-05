using System;
using System.Collections.Generic;
using System.Reflection;
using cfg.language;
using UnityEngine;
using YangTools.Scripts.Core;

/// <summary>
/// 多语言表查询管理器
/// </summary>
public class LanguageManager : MonoSingleton<LanguageManager>
{
    public cfg.language.LanguageType languageType;
    
    private Dictionary<string, PropertyInfo> languageDic = new ();
    private TBLanguage tableData;

    /// <summary>
    /// 初始化语言表和语言字段映射
    /// </summary>
    public void Start()
    {
        var tables = GameTableManager.Instance.Tables;
        if (tables == null || tables.TBLanguage == null)
        {
            Debug.LogError("配置表尚未加载，无法初始化多语言表");
            return;
        }

        tableData = tables.TBLanguage;
        Type type = typeof(Language);
        //获取所有实例属性
        PropertyInfo[] allFields = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (PropertyInfo field in allFields)
        {
            languageDic.Add(field.Name, field);
        }
    }

    /// <summary>
    /// 根据键值获取当前语言文本
    /// </summary>
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
        if (tableData == null)
        {
            return $"{key}:多语言表未初始化";
        }

        string typeStr = languageType.ToString();
        if (languageDic.TryGetValue(typeStr, out PropertyInfo fieldInfo))
        {
            Language language = tableData.GetOrDefault(key);
            if (language == null)
            {
                return $"{key}:未找到多语言值";
            }

            object value = fieldInfo.GetValue(language);
            return value?.ToString() ?? string.Empty;
        }
        return $"{key}:未找到多语言值";
    } 
}
