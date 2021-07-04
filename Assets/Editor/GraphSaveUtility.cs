using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void SaveData()
    {

    }

    public void LoadData()
    {

    }

}
