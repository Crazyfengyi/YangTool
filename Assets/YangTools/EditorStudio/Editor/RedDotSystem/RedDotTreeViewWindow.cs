#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core;

namespace GameMain
{
    public class RedDotTreeViewWindow : EditorWindow
    {
        private static RedDotTreeViewWindow redDotWindow;
        private RedDotTreeView treeView;
        private SearchField searchField;

        [MenuItem(SettingInfo.YongToolsGameToolPath + "红点树可视化窗口")]
        private static void OpenWindow()
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.DisplayDialog("警告", "编辑器未运行", "OK");
                return;
            }

            redDotWindow = GetWindow<RedDotTreeViewWindow>();
            redDotWindow.titleContent = new GUIContent("红点树视图窗口");
            redDotWindow.Show();
        }

        private void OnEnable()
        {
            treeView = new RedDotTreeView(new TreeViewState());
            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }
        private void OnDestroy()
        {
            treeView.OnDestroy();
        }
        
        /// <summary>
        /// 编辑器模式更改时
        /// </summary>
        private void OnPlayModeStateChange(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    redDotWindow.Close();
                    break;
            }
        }

        private void OnGUI()
        {
            UpToolBar();
            TreeView();
            ButtonToolBar();
        }
        /// <summary>
        /// 顶部工具栏
        /// </summary>
        private void UpToolBar()
        {
            treeView.searchString = searchField.OnGUI(new Rect(0, 0, position.width - 40f, 20f), treeView.searchString);
        }
        /// <summary>
        /// 树视图
        /// </summary>
        private void TreeView()
        {
            treeView.OnGUI(new Rect(0, 20f, position.width, position.height - 40));
        }
        /// <summary>
        /// 按钮工具栏
        /// </summary>
        private void ButtonToolBar()
        {
            GUILayout.BeginArea(new Rect(20, position.height - 18f, position.width - 40f, 16f));
            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("展开", style))
                {
                    treeView.ExpandAll();
                }
                if (GUILayout.Button("收起", style))
                {
                    treeView.CollapseAll();
                }
            }
            GUILayout.EndArea();
        }
    }
}
#endif
