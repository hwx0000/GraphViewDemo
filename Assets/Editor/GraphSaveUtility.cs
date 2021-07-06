using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSaveUtility
{
    private DialogueGraphView _dialogueGraphView;

    // 每次从类里获取edges和nodes时，都会去取graphView里的内容，并进行转型
    private List<Edge> edges => _dialogueGraphView.edges.ToList();
    private List<DialogueNode> nodes => _dialogueGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
    {
        return new GraphSaveUtility
        {
            _dialogueGraphView = graphView
        };
    }

    public void SaveData(string filePath)
    {
        if (!edges.Any())
            return;

        DialogueContainer container = ScriptableObject.CreateInstance<DialogueContainer>();

        // 遍历所有的Edge, 找到里面有Input的Edge, 组成一个数组
        Edge[] hasInputEdges = edges.Where(x => x.input.node != null).ToArray();
        // 注意, edge的Input在右边, Output在左边
        for (int i = 0; i < hasInputEdges.Length; i++)
        {
            Edge e = hasInputEdges[i];
            DialogueNode inputNode = e.input.node as DialogueNode;
            DialogueNode outputNode = e.output.node as DialogueNode;

            container.nodeLinksData.Add(new DialogueNodeLinkData()
            {
                // 注意，这里的nodeLink是以output作为开始点的
                // 这样才能保证从左到右的顺序
                baseNodeGuid = outputNode.GUID,
                portName = e.output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        Debug.Log("Edges Dif: " +  (edges.Count - hasInputEdges.Length));

        // 获取所有不为Entry的Node, 这样的Node既有input，也有output
        DialogueNode[] regularNodes = nodes.Where(x => (!x.Entry)).ToArray();
        for (int i = 0; i < regularNodes.Length; i++)
        {
            DialogueNode n = regularNodes[i];

            container.nodesData.Add(new DialogueNodeData()
            {
                nodeGuid = n.GUID,
                nodeText = n.Text,
                position = n.GetPosition().position
            });
        }

        AssetDatabase.CreateAsset(container, $"Assets/Resources/{filePath}.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadData(string path)
    {

    }

}
