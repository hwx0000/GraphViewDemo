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

    private DialogueContainer containerCache;

    public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
    {
        return new GraphSaveUtility
        {
            _dialogueGraphView = graphView
        };
    }

    /// <summary>
    /// 主要分为两步：
    /// 1. 存储所有的Edge，这里的Edge都是带有Input和Ouput的
    /// 2. 存储所有的Node，这里的Node不包含EntryNode
    /// </summary>
    /// <param name="filePath"></param>
    public void SaveData(string filePath)
    {
        if (!edges.Any())
            return;

        DialogueContainer container = ScriptableObject.CreateInstance<DialogueContainer>();

        // 遍历所有的Edge, 找到里面有Input的Edge, 组成一个数组
        // 这里默认有个前提, 第一个Edge是连接EntryNode的Edge
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

        // 应该所有的Edge都有Input吧（它毕竟是要两头的端点的）
        // 不过ghost Edge(正在拉线连接的Edge)应该没有Input
        Debug.Log("Edges Dif: " +  (edges.Count - hasInputEdges.Length));

        // 获取所有不为Entry的Node, 这样的Node既有input，也有output
        DialogueNode[] regularNodes = nodes.Where(x => (!x.Entry)).ToArray();
        // 注意, 存储的时候没有存储EntryPoint的Node, 只在NodeLinkData里存储过它的GUID
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
        // 读取的时候, EntryPoint对应的Node是不需要删除的, 只需要改它的GUID信息即可
        var entryGUID = containerCache.nodeLinksData[0].baseNodeGuid;
        DialogueNode oriEntryNode = nodes.Find(x => x.Entry);
        oriEntryNode.GUID = entryGUID;

        // 遍历现在的GraphView里的Node节点, 除了EntryNode,其他的nodes和edges全部删除
        foreach (var item in nodes)
        {
            // 除了EntryNode, 可以认为一个Edge对应一个Node
            // 因为每个Node都只有一个Input，也意味着Edge的output.node只对应一个Node
            if (item.Entry) continue;

            // 先删除所有以item为output的Node的Edge, 为啥是list
            List<Edge> es = edges.Where(x => x.output.node == item).ToList();//List<Edge>
            es.ForEach(edge => _dialogueGraphView.RemoveElement(edge));
            Debug.Log(es.Count());

            // 然后删除Node
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

            // 在NodeLinkData里寻找以此节点为BaseNode的Link的个数
            int cnt = containerCache.nodeLinksData.Where(x =>
            x.baseNodeGuid == containerCache.nodesData[i].nodeGuid).ToList().Count();

            for (int k = 0; k < cnt; k++)
                DialogueGraphView.AddOutputPort(node);
        }
    }

}
