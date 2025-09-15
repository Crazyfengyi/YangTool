using System;
using cfg.language;
using UnityEngine;
using YangTools.Scripts.Core;

public class LanguageManager : MonoSingleton<LanguageManager>
{
    public cfg.language.LanguageType languageType;
    
    public string GetLanguage(string key)
    {
        //考虑反射获取字段或其它自动化高的配表方案
        //Language temp = GameTableManager.Instance.Tables.TBLanguage.GetOrDefault(key);
        
        switch (languageType)
        {
            case cfg.language.LanguageType.Chinese:
                return GameTableManager.Instance.Tables.TBLanguage.GetOrDefault(key).Chinese;
            case cfg.language.LanguageType.English:
                return GameTableManager.Instance.Tables.TBLanguage.GetOrDefault(key).English;
            default:
                return GameTableManager.Instance.Tables.TBLanguage.GetOrDefault(key).Chinese;
        }
    } 
}
