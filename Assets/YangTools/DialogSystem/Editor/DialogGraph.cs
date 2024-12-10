/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-06-26 
*/
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// 编辑器窗口
/// </summary>
public class DialogGraph : EditorWindow
{
    private DialogGraphView m_GraphView;
    private string _fileName = "New Narrative";

    [MenuItem("YangTools/Graph/Dialog Graph")]
    public static void OpenDialogGraphWindow()
    {
        var window = GetWindow<DialogGraph>();
        window.titleContent = new GUIContent("Dialog Graph");
    }
    private void OnEnable()
    {
        ConstructGrapView();
        GenerateToolBar();
        GenerateMiniMap();
    }

    private void GenerateMiniMap()
    {
        var miniMap = new MiniMap() { anchored = true };
        miniMap.SetPosition(new Rect(10, 30, 200, 140));
        m_GraphView.Add(miniMap);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(m_GraphView);
    }
    /// <summary>
    /// 构建Grap视图
    /// </summary>
    private void ConstructGrapView()
    {
        m_GraphView = new DialogGraphView()
        {
            name = "Dialog Graph"
        };
        m_GraphView.StretchToParentSize();
        rootVisualElement.Add(m_GraphView);
    }
    /// <summary>
    /// 生成工具栏
    /// </summary>
    private void GenerateToolBar()
    {
        var toolBar = new Toolbar();
        //输入框
        var fileNameTextField = new TextField("File Name");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt =>
        {
            _fileName = evt.newValue;
        });
        toolBar.Add(fileNameTextField);
        //保存加载
        toolBar.Add(new Button(() => RequestDataOperation(true)) { text = "SaveData" });
        toolBar.Add(new Button(() => RequestDataOperation(false)) { text = "LoadData" });

        //节点创建
        var nodeCreateButton = new Button(() =>
        {
            m_GraphView.CreateNode("DialogNode");
        });
        nodeCreateButton.text = "Create Node";
        toolBar.Add(nodeCreateButton);
        rootVisualElement.Add(toolBar);
    }
    /// <summary>
    /// 请求数据操作
    /// </summary>
    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("fileName为空", "请检查fileName", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(m_GraphView);

        if (save)
        {
            saveUtility.SaveGraph(_fileName);
        }
        else
        {
            saveUtility.LoadGraph(_fileName);
        }
    }
}