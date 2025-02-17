#if UNITY_EDITOR
/*
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION# 
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-07-05 
*/
using UnityEditor;
using UnityEngine;

namespace YangTools
{
    public static class CopyAllComponent
    {
        static Component[] copiedComponents;

        [MenuItem(SettingInfo.YongToolsFunctionPath + "CopyCurrentComponents &C")]
        public static void Copy()
        {
            copiedComponents = Selection.activeGameObject.GetComponents<Component>();
        }

        [MenuItem(SettingInfo.YongToolsFunctionPath + "PasteCurrentComponents &P")]
        public static void Paste()
        {
            foreach (var targetGameObject in Selection.gameObjects)
            {
                if (!targetGameObject || copiedComponents == null) continue;
                foreach (var copiedComponent in copiedComponents)
                {
                    if (!copiedComponent) continue;
                    UnityEditorInternal.ComponentUtility.CopyComponent(copiedComponent);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetGameObject);
                }
            }
        }
    }
}
#endif