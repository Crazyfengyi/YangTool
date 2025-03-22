#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core.YangToolsManager;

namespace GameMain
{
    public class RedDotTreeViewWindow : EditorWindow
    {
        private static RedDotTreeViewWindow _window;

        private RedDotTreeView _treeView;

        private SearchField _searchField;

        [MenuItem(SettingInfo.YongToolsGameToolPath + "红点树可视化窗口")]
        private static void OpenWindow()
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.DisplayDialog("警告", "编辑器未运行", "OK");
                return;
            }

            _window = GetWindow<RedDotTreeViewWindow>();
            _window.titleContent = new GUIContent("红点树视图窗口");
            _window.Show();
        }

        private void OnEnable()
        {
            _treeView = new RedDotTreeView(new TreeViewState());

            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;

            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

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
                    
                    _window.Close();
                    
                    break;
            }
        }

        private void OnDestroy()
        {
            _treeView.OnDestroy();
        }

        private void OnGUI()
        {
            UpToolBar();
            
            TreeView();
            
            ButtonToolBar();
        }

        private void UpToolBar()
        {
            _treeView.searchString =
                _searchField.OnGUI(new Rect(0, 0, position.width - 40f, 20f), _treeView.searchString);
        }

        private void TreeView()
        {
            _treeView.OnGUI(new Rect(0, 20f, position.width, position.height - 40));
        }

        private void ButtonToolBar()
        {
            GUILayout.BeginArea(new Rect(20, position.height - 18f, position.width - 40f, 16f));

            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";

                if (GUILayout.Button("展开", style))
                {
                    _treeView.ExpandAll();
                }

                if (GUILayout.Button("收起", style))
                {
                    _treeView.CollapseAll();
                }
            }
            
            GUILayout.EndArea();
        }
    }
}
#endif
