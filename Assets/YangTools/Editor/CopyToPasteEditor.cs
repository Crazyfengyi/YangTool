using System;
using System.Reflection;
namespace YangTools.Editor
{
    public class CopyToPasteEditor
    {
        private static object fromScript;//复制源脚本
        private static Type copyType;//脚本类型
        private static FieldInfo[] copyField;//复制的字段
        public void Copy(object _fromScript)
        {
            copyType = _fromScript.GetType();//类型
            copyField = copyType.GetFields(BindingFlags.Public| BindingFlags.Instance);
        }
        public void Paste(object toScript)
        {
            foreach (var item in copyField)
            {
                object value = item.GetValue(fromScript);
                item.SetValue(toScript,value);
            }
        }
    }
}