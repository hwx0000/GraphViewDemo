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

    /// <summary>
    /// ��Ҫ��Ϊ������
    /// 1. �洢���е�Edge�������Edge���Ǵ���Input��Ouput��
    /// 2. �洢���е�Node�������Node������EntryNode
    /// </summary>
    /// <param name="filePath"></param>
    public void SaveData(string filePath)
    {
        if (!edges.Any())
            return;

        DialogueContainer container = ScriptableObject.CreateInstance<DialogueContainer>();

        // �������е�Edge, �ҵ�������Input��Edge, ���һ������
        // ����Ĭ���и�ǰ��, ��һ��Edge������EntryNode��Edge
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

        // �������ڵ�GraphView���Node�ڵ�, ����EntryNode,������nodes��edgesȫ��ɾ��
        foreach (var item in nodes)
        {
            // ����EntryNode, ������Ϊһ��Edge��Ӧһ��Node
            // ��Ϊÿ��Node��ֻ��һ��Input��Ҳ��ζ��Edge��output.nodeֻ��Ӧһ��Node
            if (item.Entry) continue;

            // ��ɾ��������itemΪoutput��Node��Edge, Ϊɶ��list
            List<Edge> es = edges.Where(x => x.output.node == item).ToList();//List<Edge>
            es.ForEach(edge => _dialogueGraphView.RemoveElement(edge));
            Debug.Log(es.Count());

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
                title = containerCache.nodesData[i].nodeText
            };

            Vector2 pos = containerCache.nodesData[i].position;
            Rect rect = node.GetPosition();
            rect.x = pos.x; 
            rect.y = pos.y;
            node.SetPosition(rect);

            _dialogueGraphView.AddElement(node);

            // ��NodeLinkData��Ѱ���Դ˽ڵ�ΪBaseNode��Link�ĸ���
            int cnt = containerCache.nodeLinksData.Where(x =>
            x.baseNodeGuid == containerCache.nodesData[i].nodeGuid).ToList().Count();

            for (int k = 0; k < cnt; k++)
                DialogueGraphView.AddOutputPort(node);
        }
    }

}
