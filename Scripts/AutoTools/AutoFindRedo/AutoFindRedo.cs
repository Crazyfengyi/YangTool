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
        //�Ƿ�֧�ֶ�ѡ��
        tree.Selection.SupportsMultiSelect = false;

        tree.Add("FindRedo", new OneKeySearchDuplicateFiles());

        //����·���µ����м̳�ScriptableObject�Ľű���
        //tree.AddAllAssetsAtPath("Odin Settings", "Assets/Plugins/Sirenix", typeof(ScriptableObject), true, true);
        return tree;
    }
}
