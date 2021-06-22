using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView: GraphView
{
    public DialogueGraphView() 
    {
        //// 允许拖拽Content
        //this.AddManipulator(new ContentDragger());
        // 允许拖拽Selection里的内容
        this.AddManipulator(new SelectionDragger());
        // GraphView允许进行框选
        this.AddManipulator(new RectangleSelector());


        // 创建StartNode
        var startNode = GenEntryPointNode();
        // 并加入到GraphView里
        AddElement(startNode);
        // 给StartNode添加Port
        var port = GenPortForNode(startNode, Direction.Output);
        port.portName = "Next";
        startNode.outputContainer.Add(port);

        startNode.RefreshExpandedState();
        startNode.RefreshPorts();
    }

    private DialogueNode GenEntryPointNode() 
    {
        DialogueNode node = new DialogueNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            Text = "ENTRYPOINT",
            Entry = true
        };
        node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));

        return node;
     }

    // 为节点n创建input port或者output port
    // Direction: 是一个简单的枚举，分为Input和Output两种
    private Port GenPortForNode(Node n, Direction portDir, Port.Capacity capacity = Port.Capacity.Single) 
    {
        // Orientation也是个简单的枚举，分为Horizontal和Vertical两种
        return n.InstantiatePort(Orientation.Horizontal, portDir, capacity, typeof(float));
    }

    public void AddDialogueNode(string nodeName)
    {
        // 1. 创建Node
        DialogueNode node = new DialogueNode
        {
            title = nodeName,
            GUID = Guid.NewGuid().ToString(),
            Text = nodeName,
            Entry = false
        };
        node.SetPosition(new Rect(x: 100, y: 200, width: 100, height: 150));

        // 2. 为其创建InputPort
        var iport = GenPortForNode(node, Direction.Input, Port.Capacity.Multi);
        iport.portName = "input";
        node.inputContainer.Add(iport);
        node.RefreshExpandedState();
        node.RefreshPorts();

        // 3. 为其在title上创建btn, 点击btn时会调用函数
        Button btn = new Button(() =>
        {
            AddOutputPort(node);
        });
        btn.text = "Add Output Port";
        node.titleContainer.Add(btn);

        AddElement(node);
    }
    
    private void AddOutputPort(DialogueNode node)
    {
        var outPort = GenPortForNode(node, Direction.Output);

        // 根据node的outport的数目给新的outport命名
        var count = node.outputContainer.Query("connector").ToList().Count;
        string name = $"Output {count}";
        outPort.portName = name;
        node.outputContainer.Add(outPort);
        node.RefreshExpandedState();
        node.RefreshPorts();
    }

    // =============== 在DialogueGraphView类里 =============
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter adapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        // 继承的GraphView里有个Property：ports, 代表graph里所有的port
        ports.ForEach((port) =>
        {
            // 对每一个在graph里的port，进行判断，这里有两个规则：
            // 1. port不可以与自身相连
            // 2. 同一个节点的port之间不可以相连
            if (port != startPort && port.node != startPort.node)
            {
                compatiblePorts.Add(port);
            }
        });

        // 在我理解，这个函数就是把所有除了startNode里的port都收集起来，放到了List里
        // 所以这个函数能让StartNode的Output port与任何其他的Node的Input port相连（output port应该默认不能与output port相连吧）
        return compatiblePorts;
    }
}
