/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:DESKTOP-JVG8VG4
 *UnityVersion：6000.0.17f1c1
 *创建时间:2025-02-17
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using YangTools;
using YangTools.UGUI;

namespace YangTools
{
    //组件图标
    public abstract class CustomHierarchyWindow
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private static readonly List<Type> Types = new List<Type>();
        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;
            Types.Clear();
            var components = go.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null) continue;
                if (Types.Contains(component.GetType())) return;
                Types.Add(component.GetType());
                
                Texture texture = AssetPreview.GetMiniTypeThumbnail(component.GetType()) ??
                                  AssetPreview.GetMiniThumbnail(component);
                if(texture == null) continue;
                Rect rect = selectionRect;
                rect.x += selectionRect.width - (i + 1) * 20f;
                rect.width = 20f;
                GUI.Label(rect,new GUIContent(texture,component.GetType().Name));
            }
        }
    }

    //资源依赖
    public abstract class CustomProjectWindow
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            string asstPath = AssetDatabase.GUIDToAssetPath(guid);
            //TODO: EditorUtility.CollectDependencies()//游戏对象的依赖
            string[] dependencies = AssetDatabase.GetDependencies(asstPath);
            if(dependencies.Length == 0)return;
            Rect rect = selectionRect;
            rect.x += selectionRect.width - 20f;
            rect.width = 20f;
            GUI.Label(rect,new GUIContent(dependencies.Length.ToString(),"依赖"));
        }
    }
}