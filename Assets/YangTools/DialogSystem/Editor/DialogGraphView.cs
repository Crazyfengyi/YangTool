/*
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-06-25 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 对话图表
/// </summary>
public class DialogGraphView : GraphView
{
    //默认节点大小
    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    public DialogGraphView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogGraphSetting"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        //内置设置
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        //背景设置
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        //生成入口节点
        AddElement(GenerateEntryPointNode());
    }
    /// <summary>
    /// 能兼容的端口
    /// </summary>
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }
    /// <summary>
    /// 生成入口节点
    /// </summary>
    private DialogNode GenerateEntryPointNode()
    {
        var node = new DialogNode()
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            DialogText = "Start",
            EntryPoint = true
        };

        var generatePort = GeneratePort(node, Direction.Output);
        generatePort.portName = "Next";
        node.outputContainer.Add(generatePort);

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }
    /// <summary>
    /// 创建节点
    /// </summary>
    public void CreateNode(string nodeName)
    {
        AddElement(CreateDialogNode(nodeName));
    }
    /// <summary>
    /// 创建对话框节点
    /// </summary>
    public DialogNode CreateDialogNode(string nodeName)
    {
        var dialogNode = new DialogNode()
        {
            title = nodeName,
            DialogText = nodeName,
            GUID = Guid.NewGuid().ToString()
        };
        var inputPort = GeneratePort(dialogNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogNode.inputContainer.Add(inputPort);

        dialogNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        var button = new Button(() =>
        {
            AddChoicePort(dialogNode);
        });
        button.text = "NewChoice";
        dialogNode.titleContainer.Add(button);

        var textFiled = new TextField(String.Empty);
        textFiled.RegisterValueChangedCallback(evt =>
        {
            dialogNode.DialogText = evt.newValue;
            dialogNode.title = evt.newValue;
        });
        textFiled.SetValueWithoutNotify(dialogNode.title);
        dialogNode.mainContainer.Add(textFiled);

        dialogNode.RefreshExpandedState();
        dialogNode.RefreshPorts();
        dialogNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

        return dialogNode;
    }

    /// <summary>
    /// 添加选择端口
    /// </summary>
    /// <param name="dialogNode">节点</param>
    public void AddChoicePort(DialogNode dialogNode, string overriddenPortName = "")
    {
        var generatePort = GeneratePort(dialogNode, Direction.Output);

        var oldLabel = generatePort.contentContainer.Q<Label>("type");
        generatePort.contentContainer.Remove(oldLabel);

        var outputPortCount = dialogNode.outputContainer.Query("connector").ToList().Count;

        var choicePortName = string.IsNullOrEmpty(overriddenPortName)
            ? $"Choice {outputPortCount + 1 }"
            : overriddenPortName;
        generatePort.portName = choicePortName;

        var textField = new TextField()
        {
            name = string.Empty,
            value = choicePortName
        };

        textField.RegisterValueChangedCallback(evt => generatePort.portName = evt.newValue);
        generatePort.contentContainer.Add(new Label());
        generatePort.contentContainer.Add(textField);
        var deleteButton = new Button(() => RemovePort(dialogNode, generatePort))
        {
            text = "X"
        };
        generatePort.contentContainer.Add(deleteButton);

        dialogNode.outputContainer.Add(generatePort);
        dialogNode.RefreshExpandedState();
        dialogNode.RefreshPorts();
    }

    private void RemovePort(DialogNode dialogNode, Port generatePort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatePort.portName && x.output.node == generatePort.node);
        if (!targetEdge.Any()) return;

        var edge = targetEdge.First();
        edge.input.Disconnect(edge);
        RemoveElement(targetEdge.First());

        dialogNode.outputContainer.Remove(generatePort);
        dialogNode.RefreshPorts();
        dialogNode.RefreshExpandedState();
    }

    /// <summary>
    /// 创建端口
    /// </summary>
    /// <param name="node">节点</param>
    /// <param name="portDirection">输入/输出</param>
    /// <param name="capacity">容量(连接数)</param>
    private Port GeneratePort(DialogNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }
}