using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AutoFindRedo : OdinMenuEditorWindow
{
    [MenuItem("YangTools/OdinExtendWindow")]
    private static void OpenWindow()
    {
        GetWindow<AutoFindRedo>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new OdinMenuTree();
        //是否支持多选择
        tree.Selection.SupportsMultiSelect = false;

        tree.Add("FindRedo", new OneKeySearchDuplicateFiles());

        //加入路径下的所有继承ScriptableObject的脚本？
        //tree.AddAllAssetsAtPath("Odin Settings", "Assets/Plugins/Sirenix", typeof(ScriptableObject), true, true);
        return tree;
    }
}
