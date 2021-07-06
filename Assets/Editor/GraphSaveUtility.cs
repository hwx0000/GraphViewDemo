using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSaveUtility
{
    private DialogueGraphView _dialogueGraphView;

    // ÿ�δ������ȡedges��nodesʱ������ȥȡgraphView������ݣ�������ת��
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

        // �������е�Edge, �ҵ�������Input��Edge, ���һ������
        Edge[] hasInputEdges = edges.Where(x => x.input.node != null).ToArray();
        // ע��, edge��Input���ұ�, Output�����
        for (int i = 0; i < hasInputEdges.Length; i++)
        {
            Edge e = hasInputEdges[i];
            DialogueNode inputNode = e.input.node as DialogueNode;
            DialogueNode outputNode = e.output.node as DialogueNode;

            container.nodeLinksData.Add(new DialogueNodeLinkData()
            {
                // ע�⣬�����nodeLink����output��Ϊ��ʼ���
                // �������ܱ�֤�����ҵ�˳��
                baseNodeGuid = outputNode.GUID,
                portName = e.output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        Debug.Log("Edges Dif: " +  (edges.Count - hasInputEdges.Length));

        // ��ȡ���в�ΪEntry��Node, ������Node����input��Ҳ��output
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
