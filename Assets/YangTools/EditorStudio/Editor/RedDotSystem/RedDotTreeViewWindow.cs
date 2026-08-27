#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core;

namespace GameMain
{
    /// <summary>
    /// 红点树可视化窗口
    /// </summary>
    public class RedDotTreeViewWindow : EditorWindow
    {
        private static RedDotTreeViewWindow redDotWindow; //当前窗口
        private RedDotTreeView treeView; //红点树视图
        private SearchField searchField; //搜索输入框

        /// <summary>
        /// 打开红点树窗口
        /// </summary>
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

        /// <summary>
        /// 初始化窗口内容和事件
        /// </summary>
        private void OnEnable()
        {
            treeView = new RedDotTreeView(new TreeViewState<int>());
            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        /// <summary>
        /// 窗口停用时释放事件
        /// </summary>
        private void OnDisable()
        {
            if (searchField != null && treeView != null)
            {
                searchField.downOrUpArrowKeyPressed -= treeView.SetFocusAndEnsureSelectedItem;
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            treeView?.OnDestroy();
        }

        /// <summary>
        /// 销毁窗口时清理静态引用
        /// </summary>
        private void OnDestroy()
        {
            if (redDotWindow == this)
            {
                redDotWindow = null;
            }
        }

        /// <summary>
        /// 编辑器模式更改时
        /// </summary>
        /// <param name="stateChange">播放模式状态</param>
        private void OnPlayModeStateChange(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    Close();
                    break;
            }
        }

        /// <summary>
        /// 绘制窗口内容
        /// </summary>
        private void OnGUI()
        {
            if (treeView == null || searchField == null)
            {
                return;
            }

            UpToolBar();
            TreeView();
            ButtonToolBar();
        }
        /// <summary>
        /// 顶部工具栏
        /// </summary>
        private void UpToolBar()
        {
            float width = Mathf.Max(0f, position.width - 40f);
            treeView.searchString = searchField.OnGUI(new Rect(0f, 0f, width, 20f), treeView.searchString);
        }
        /// <summary>
        /// 树视图
        /// </summary>
        private void TreeView()
        {
            float height = Mathf.Max(0f, position.height - 40f);
            treeView.OnGUI(new Rect(0f, 20f, position.width, height));
        }
        /// <summary>
        /// 按钮工具栏
        /// </summary>
        private void ButtonToolBar()
        {
            float width = Mathf.Max(0f, position.width - 40f);
            GUILayout.BeginArea(new Rect(20f, position.height - 18f, width, 16f));
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
