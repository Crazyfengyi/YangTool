#if UNITY_EDITOR
/* 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-06-15 
*/
using DG.DemiEditor;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(Transform), true)]//bool 子类是否可用
    public class ShowTransformPos : Editor
    {
        Transform tagetTranform;//自身
        SerializedObject projectile;

        private void OnEnable()
        {
            tagetTranform = target as Transform;
            projectile = new SerializedObject(target);
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();//垂直布局
            tagetTranform.localPosition = EditorGUILayout.Vector3Field("Position", tagetTranform.localPosition);//三维输入
            tagetTranform.localEulerAngles = EditorGUILayout.Vector3Field("Rotation", tagetTranform.localEulerAngles);
            tagetTranform.localScale = EditorGUILayout.Vector3Field("Scale", tagetTranform.localScale);
            tagetTranform.position = EditorGUILayout.Vector3Field("世界坐标：", tagetTranform.position);
            EditorGUILayout.EndVertical();

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
