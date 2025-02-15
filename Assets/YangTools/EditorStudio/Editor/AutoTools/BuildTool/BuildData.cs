using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildData : ScriptableObject
{
    public string showName;
    public List<DefineSymbol> defineList = new List<DefineSymbol>();
    public string DefineStr => string.Join(";", defineList.Select(item=>item.Name).ToArray());
    
    public BuildTargetGroup buildTarget; 
    public string buildPath;//打包路径
    public string companyName;//公司名称
    public string buildName;//包名
    public string buildVersion;//版本号
    public int buildNumberCode;//内部版本号
    
    public string bundleVersion;
    public string adsId;
    public string tips;
}

[System.Serializable]
public class DefineSymbol
{
    public bool IsOpen;
    public string Name;
    public string Desc;
    public string Group;

    public DefineSymbol Clone()
    {
        return new DefineSymbol()
        {
            Name = Name,
            Desc = Desc,
        };
    }
}
