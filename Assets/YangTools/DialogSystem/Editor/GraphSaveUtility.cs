/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-06-26 
*/
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogGraphView _targetGraphView;
    private DialogContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DialogNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogGraphView dialogGraphView)
    {
        return new GraphSaveUtility()
        {
            _targetGraphView = dialogGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        if (!Edges.Any())
        {
            return;
        }

        var dialogContainer = ScriptableObject.CreateInstance<DialogContainer>();

        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogNode;
            var inputNode = connectedPorts[i].input.node as DialogNode;

            dialogContainer.NodeLinks.Add(new NodeLinkData()
            {
                BaseNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                TargetNodeGuid = inputNode.GUID,
            });
        }

        foreach (var dialogNode in Nodes.Where(node => !node.EntryPoint))
        {
            dialogContainer.DialogNodeData.Add(new DialogNodeData()
            {
                Guid = dialogNode.GUID,
                DialogText = dialogNode.DialogText,
                Positoin = dialogNode.GetPosition().position
            });
        }

        AssetDatabase.CreateAsset(dialogContainer, $"Assets/YangTools/DialogSystem/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<DialogContainer>(fileName);
        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("路径没找到", "目标没找到", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }
    private void ClearGraph()
    {
        Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGuid;
        foreach (var node in Nodes)
        {
            if (node.EntryPoint) continue;
            //移除edges连接输入节点
            Edges.Where(x => x.input.node == node).ToList()
                .ForEach(edge => _targetGraphView.RemoveElement(edge));
            //删除自己
            _targetGraphView.RemoveElement(node);
        }
    }
    private void CreateNodes()
    {
        foreach (var nodeData in _containerCache.DialogNodeData)
        {
            var tempNode = _targetGraphView.CreateDialogNode(nodeData.DialogText);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.PortName));
        }
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].TargetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(
                    _containerCache.DialogNodeData.First(x => x.Guid == targetNodeGuid).Positoin,
                    _targetGraphView.defaultNodeSize
                    ));
            }

        }
    }

    private void LinkNodes(Port ouput, Port input)
    {
        var tempEdge = new Edge()
        {
            output = ouput,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }
}