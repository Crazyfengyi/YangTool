/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-06-15 
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using YangTools.EditorStudio;

namespace YangTools
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(Transform), true)]//bool 子类是否可用
    public class ShowTransformPos : Editor
    {
        Transform tagetTranform;//自身
        private void OnEnable()
        {
            tagetTranform = target as Transform;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();//垂直布局
            tagetTranform.localPosition = EditorGUILayout.Vector3Field("Position", tagetTranform.localPosition);//三维输入
            tagetTranform.localEulerAngles = EditorGUILayout.Vector3Field("Rotation", tagetTranform.localEulerAngles);
            tagetTranform.localScale = EditorGUILayout.Vector3Field("Scale", tagetTranform.localScale);
            tagetTranform.position = EditorGUILayout.Vector3Field("世界坐标：", tagetTranform.position);
            EditorGUILayout.EndVertical();
        }
    }
}
