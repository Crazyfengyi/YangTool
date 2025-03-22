#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using YangTools.Scripts.Core;

namespace YangTools
{
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
            //是否支持多选择
            tree.Selection.SupportsMultiSelect = false;
            tree.Add("FindRedo", new OneKeySearchDuplicateFiles());

            //加入路径下的所有继承ScriptableObject的脚本？
            //tree.AddAllAssetsAtPath("Odin Settings", "Assets/Plugins/Sirenix", typeof(ScriptableObject), true, true);
            return tree;
        }
    }
}
#endif
