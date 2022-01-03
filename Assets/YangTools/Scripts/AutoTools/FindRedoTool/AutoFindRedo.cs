using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using YangTools;

public class AutoFindRedo : OdinMenuEditorWindow
{
    [MenuItem(SettingInfo.MenuPath + "OdinExtendWindow")]
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
