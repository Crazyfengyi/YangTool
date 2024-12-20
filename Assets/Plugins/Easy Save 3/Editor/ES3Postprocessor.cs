﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/*
 * ---- How Postprocessing works for the reference manager ----
 * - When the manager is first added to the scene, all top-level dependencies are added to the manager (AddManagerToScene).
 * - When the manager is first added to the scene, all prefabs with ES3Prefab components are added to the manager (AddManagerToScene).
 * - All GameObjects and Components in the scene are added to the reference manager when we enter Playmode or the scene is saved (PlayModeStateChanged, OnWillSaveAssets -> AddGameObjectsAndComponentstoManager).
 * - When a UnityEngine.Object field of a Component is modified, the new UnityEngine.Object reference is added to the reference manager (PostProcessModifications)
 * - All prefabs with ES3Prefab Components are added to the reference manager when we enter Playmode or the scene is saved (PlayModeStateChanged, OnWillSaveAssets -> AddGameObjectsAndComponentstoManager).
 * - Local references for prefabs are processed whenever a prefab with an ES3Prefab Component is deselected (SelectionChanged -> ProcessGameObject)
 */
[InitializeOnLoad]
public class ES3Postprocessor : UnityEditor.AssetModificationProcessor
{
    private static bool refreshed = false;

    public static ES3ReferenceMgr RefMgr
    {
        get { return (ES3ReferenceMgr)ES3ReferenceMgr.Current; }
    }

    public static GameObject lastSelected = null;


    // This constructor is also called once when playmode is activated and whenever recompilation happens
    // because we have the [InitializeOnLoad] attribute assigned to the class.
    static ES3Postprocessor()
    {
        // Open the Easy Save 3 window the first time ES3 is installed.
        ES3Editor.ES3Window.OpenEditorWindowOnStart();

#if UNITY_2017_2_OR_NEWER
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
#else
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
#endif
    }

    #region Reference Updating

    private static void RefreshReferences(bool isEnteringPlayMode = false)
    {
        if (refreshed) // If we've already refreshed, do nothing.
            return;

        if (RefMgr != null && ES3Settings.defaultSettingsScriptableObject.autoUpdateReferences)
        {
            RefMgr.RefreshDependencies(isEnteringPlayMode);
        }
        UpdateAssembliesContainingES3Types();
        refreshed = true;
    }

#if UNITY_2017_2_OR_NEWER
    public static void PlayModeStateChanged(PlayModeStateChange state)
    {
        // Add all GameObjects and Components to the reference manager before we enter play mode.
        if (state == PlayModeStateChange.ExitingEditMode && ES3Settings.defaultSettingsScriptableObject.autoUpdateReferences)
        {
            RefreshReferences(true);
        }
    }
#else
    public static void PlaymodeStateChanged()
    {
        // Add all GameObjects and Components to the reference manager before we enter play mode.
        if (!EditorApplication.isPlaying && ES3Settings.defaultSettingsScriptableObject.autoUpdateReferences)
            RefreshReferences(true);
    }
#endif


    public static string[] OnWillSaveAssets(string[] paths)
    {
        // Don't refresh references when the application is playing.
        if (!Application.isPlaying && ES3Settings.defaultSettingsScriptableObject.autoUpdateReferences)
            RefreshReferences();
        return paths;
    }

    #endregion


    private static void UpdateAssembliesContainingES3Types()
    {
#if UNITY_2017_3_OR_NEWER
        var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
        var defaults = ES3Settings.defaultSettingsScriptableObject;
        var assemblyNames = new List<string>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var name = assembly.name;
                var substr = name.Length >= 5 ? name.Substring(0, 5) : "";

                if (substr != "Unity" && substr != "com.u" && !name.Contains("-Editor"))
                    assemblyNames.Add(name);
            }
            catch { }
        }

        defaults.settings.assemblyNames = assemblyNames.ToArray();
        EditorUtility.SetDirty(defaults);
#endif
    }

    public static GameObject AddManagerToScene()
    {
        GameObject mgr = null;
        if (RefMgr != null)
            mgr = RefMgr.gameObject;

        if (mgr == null)
        {
            mgr = new GameObject("Easy Save 3 Manager");
            var inspectorInfo = mgr.AddComponent<ES3InspectorInfo>();
            inspectorInfo.message = "The Easy Save 3 Manager is required in any scenes which use Easy Save, and is automatically added to your scene when you enter Play mode.\n\nTo stop this from automatically being added to your scene, go to 'Window > Easy Save 3 > Settings' and deselect the 'Auto Add Manager to Scene' checkbox.";
        }

        if (mgr.GetComponent<ES3ReferenceMgr>() == null)
        {
            mgr.AddComponent<ES3ReferenceMgr>().RefreshDependencies();
            RefMgr.RefreshDependencies();
        }

        if (mgr.GetComponent<ES3AutoSaveMgr>() == null)
            mgr.AddComponent<ES3AutoSaveMgr>();

        Undo.RegisterCreatedObjectUndo(mgr, "Enabled Easy Save for Scene");
        return mgr;
    }
}