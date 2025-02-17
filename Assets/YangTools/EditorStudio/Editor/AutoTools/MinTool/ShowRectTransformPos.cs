#if UNITY_EDITOR
/* 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-06-15 
*/
using System;
using System.Linq;
using System.Reflection;
using DG.DemiEditor;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(RectTransform), false)]//子类是否可用
    public class ShowRectTransformPos : Editor
    {
        RectTransform targetTransform;//自身
        SerializedObject projectile;

        private bool isRectTransform;
        private Editor instanceEditor;//编辑器
        private MethodInfo onSceneGUI;
        private void OnEnable()
        {
            targetTransform = target as RectTransform;
            
            isRectTransform = target as RectTransform;
            projectile = new SerializedObject(target);
        
            var editorType = Assembly.GetAssembly(typeof(Editor)).GetTypes()
                .FirstOrDefault(m => m.Name == (isRectTransform ? "RectTransformEditor" : "TransformInspector"));
            instanceEditor = CreateEditor(targets, editorType);
            onSceneGUI = editorType?.GetMethod("OnSceneGUI", BindingFlags.NonPublic |
                                                             BindingFlags.Instance| BindingFlags.Static|
                                                             BindingFlags.Public);
        }

        private void OnSceneGUI()
        {
            if (isRectTransform && instanceEditor)
            {
                onSceneGUI?.Invoke(instanceEditor, null);
            }
        }

        private void OnDisable()
        {
            if (instanceEditor)
            {
                DestroyImmediate(instanceEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            instanceEditor?.OnInspectorGUI();
            
            //EditorGUILayout.BeginVertical();//垂直布局
            targetTransform.position = EditorGUILayout.Vector3Field("世界坐标：", targetTransform.position);
            //EditorGUILayout.EndVertical();

            //SerializedProperty iter = projectile.GetIterator();
            //iter.Next(true);
            ////序列化手动干预
            //while (iter.NextVisible(false))
            //{
            //    bool draw = true;
            //    //Debug.LogError(iter.name);
            //    if (iter.name == "m_LocalPosition")//这个属性更具Draw是否渲染
            //    {
            //        draw =  projectile.FindProperty("m_ConstrainProportionsScale").boolValue;
            //    }
            //    if(draw)EditorGUILayout.PropertyField(iter,iter.isExpanded);
            //}

            //projectile.ApplyModifiedProperties();
        }
    }
}
#endif
