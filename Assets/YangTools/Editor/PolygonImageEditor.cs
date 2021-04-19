/** 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     动态更改图片显示边数--RawImageEditor编辑器功能重写
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-06-13 
*/

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(PolygonImage), true)]
[CanEditMultipleObjects]
public class UICircleInspector : RawImageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PolygonImage circle = target as PolygonImage;
        circle.segments = Mathf.Clamp(EditorGUILayout.IntField("UICircle多边形", circle.segments), 3, 360);//设置边数的最小于最大值（3-360）
    }

    [MenuItem("GameObject/UI/PolygonRawImage")]
    static void CreatPolygonImage()
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("PolygonImage", typeof(PolygonImage));
                go.GetComponent<PolygonImage>().raycastTarget = true;
                go.transform.SetParent(Selection.activeTransform);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.zero;
            }
        }
    }

}