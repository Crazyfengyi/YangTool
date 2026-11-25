using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
 
/// <summary>
/// 绘制按钮扩展组件的条件化事件配置面板
/// </summary>
[CustomEditor(typeof(BtnExtension))]
public class BtnExtensionEditor : UnityEditor.UI.ButtonEditor    
{
    /// <summary>
    /// 根据交互开关绘制相应的按钮参数和事件
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BtnExtension myScript = (BtnExtension)target;
        //创建一个打开/关闭的开关按钮
        myScript.singleClickEnabled = GUILayout.Toggle(myScript.singleClickEnabled, "单击");
        myScript.doubleClickEnabled = GUILayout.Toggle(myScript.doubleClickEnabled, "双击");
        if (myScript.doubleClickEnabled)
        {
            myScript.doubleClickTime = EditorGUILayout.FloatField("双击时间", myScript.doubleClickTime);
        }
        myScript.longPressEnabled = GUILayout.Toggle(myScript.longPressEnabled, "长按");
        if (myScript.longPressEnabled)
        {
            myScript.minPressTime = EditorGUILayout.FloatField("最小按住时间", myScript.minPressTime);
        }
        myScript.useSelfAni = GUILayout.Toggle(myScript.useSelfAni, "使用自定义动画");
    }
}
