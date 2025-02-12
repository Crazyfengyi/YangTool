#if UNITY_EDITOR
/*
*Copyright(C) 2020 by DefaultCompany 
*All rights reserved. 
*Author:       DESKTOP-AJS8G4U 
*UnityVersion：2022.1.0b14 
*创建时间:         2022-04-07 
*/

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    /// <summary>
    /// Hierarchy面板设置
    /// </summary>
    [InitializeOnLoad]
    public class HierarchyShow : MonoBehaviour
    {
        private static readonly Vector2 Offset = new Vector2(0, 2);
        
        static HierarchyShow()
        {
            //开关注册回调
            if (SettingInfo.GetSO<YangSettingSO>().isOpenHierarchyShowSetting)
            {
                EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
            }
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var fontColor = Color.yellow;
            var backgroundColor = new Color(.22f, .22f, .22f);

            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null) return;
            
            PrefabType prefabType = PrefabUtility.GetPrefabType(obj);
            if (prefabType == PrefabType.PrefabInstance)
            {
                if (Selection.instanceIDs.Contains(instanceID))
                {
                    fontColor = Color.white;
                    backgroundColor = new Color(0.24f, 0.48f, 0.90f);
                }

                Rect offsetRect = new Rect(selectionRect.position + Offset, selectionRect.size);
                EditorGUI.DrawRect(selectionRect, backgroundColor);
                EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = fontColor },
                    fontStyle = FontStyle.Bold
                });
            }
        }
    }
}
#endif