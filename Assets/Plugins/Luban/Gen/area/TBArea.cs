//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;
using SimpleJSON;



namespace cfg.area
{ 

public sealed partial class TBArea
{
    private readonly Dictionary<int, area.Tips> _dataMap;
    private readonly List<area.Tips> _dataList;
    
    public TBArea(JSONNode _json)
    {
        _dataMap = new Dictionary<int, area.Tips>();
        _dataList = new List<area.Tips>();
        
        foreach(JSONNode _row in _json.Children)
        {
            var _v = area.Tips.DeserializeTips(_row);
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public Dictionary<int, area.Tips> DataMap => _dataMap;
    public List<area.Tips> DataList => _dataList;

    public area.Tips GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public area.Tips Get(int key) => _dataMap[key];
    public area.Tips this[int key] => _dataMap[key];

    public void Resolve(Dictionary<string, object> _tables)
    {
        foreach(var v in _dataList)
        {
            v.Resolve(_tables);
        }
        PostResolve();
    }

    public void TranslateText(System.Func<string, string, string> translator)
    {
        foreach(var v in _dataList)
        {
            v.TranslateText(translator);
        }
    }
    
    
    partial void PostInit();
    partial void PostResolve();
}

}