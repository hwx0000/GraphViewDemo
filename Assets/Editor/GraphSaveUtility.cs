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

    private DialogueContainer containerCache;

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

        // Ӧ�����е�Edge����Input�ɣ����Ͼ���Ҫ��ͷ�Ķ˵�ģ�
        // ����ghost Edge(�����������ӵ�Edge)Ӧ��û��Input
        Debug.Log("Edges Dif: " +  (edges.Count - hasInputEdges.Length));

        // ��ȡ���в�ΪEntry��Node, ������Node����input��Ҳ��output
        DialogueNode[] regularNodes = nodes.Where(x => (!x.Entry)).ToArray();
        // ע��, �洢��ʱ��û�д洢EntryPoint��Node, ֻ��NodeLinkData��洢������GUID
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
        containerCache = AssetDatabase.LoadAssetAtPath<DialogueContainer>($"Assets/Resources/{path}.asset");
        if (containerCache == null)
        {
            EditorUtility.DisplayDialog("Wrong", "Empty Path!", "OK");
            Debug.Log($"Assets/Resources/{path}.asset");
            
            return;
        }

        // Clear Graph
        // ��ȡ��ʱ��, EntryPoint��Ӧ��Node�ǲ���Ҫɾ����, ֻ��Ҫ������GUID��Ϣ����
        var entryGUID = containerCache.nodeLinksData[0].baseNodeGuid;
        DialogueNode oriEntryNode = nodes.Find(x => x.Entry);
        oriEntryNode.GUID = entryGUID;

        foreach (var item in nodes)
        {
            // ����EntryNode, ������Ϊһ��Edge��Ӧһ��Node, ����Edge��output.node��Ӧһ��Node
            if (item.Entry) break;

            // ��ɾ��������itemΪouypuy��Node��Edge
            edges.Where(x => x.output.node == item).ToList()//List<Edge>
                .ForEach(edge => _dialogueGraphView.RemoveElement(edge));

            // Ȼ��ɾ��Node
            _dialogueGraphView.RemoveElement(item);
        }

        // Create Nodes
        for (int i = 0; i < containerCache.nodesData.Count; i++)
        {
            DialogueNode node = new DialogueNode()
            {
                GUID = containerCache.nodesData[i].nodeGuid,
                Text = containerCache.nodesData[i].nodeText,
            };

            Vector2 pos = containerCache.nodesData[i].position;
            Rect rect = node.GetPosition();
            rect.x = pos.x; 
            rect.y = pos.y;
            node.SetPosition(rect);

            _dialogueGraphView.AddElement(node);
        }


    }

}
