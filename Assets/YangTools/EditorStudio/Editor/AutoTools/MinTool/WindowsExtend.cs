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

        static List<Type> types = new List<Type>();
        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;
            types.Clear();
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null) continue;
                if (types.Contains(component.GetType())) return;
                types.Add(component.GetType());
                
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
            string[] dependencies = AssetDatabase.GetDependencies(asstPath);
            if(dependencies.Length == 0)return;
            Rect rect = selectionRect;
            rect.x += selectionRect.width - 20f;
            rect.width = 20f;
            GUI.Label(rect,new GUIContent(dependencies.Length.ToString(),"依赖"));
        }
    }
}