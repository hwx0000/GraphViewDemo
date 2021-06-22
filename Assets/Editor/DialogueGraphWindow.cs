using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphWindow : EditorWindow
{
    private DialogueGraphView _graphView;

    [MenuItem("Graph/Open Dialogue Graph View")]
    public static void OpenGraphViewWindow() 
    {
        // 获取自身对应的窗口，创建一个窗口
        var window = GetWindow<DialogueGraphWindow>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable() 
    {
        _graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };

        // 让graphView铺满整个Editor窗口
        _graphView.StretchToParentSize();
        // 把它添加到EditorWindow的可视化Root元素下面
        rootVisualElement.Add(_graphView);

        //  相关内容涉及到菜单设置，所以应该放到DialogueGraphWindow类下
        // 这个Toolbar类在UnityEditor.UIElements下
        Toolbar toolbar = new Toolbar();
        //创建lambda函数，代表点击按钮后发生的函数调用
        Button btn = new Button(clickEvent: () => { _graphView.AddDialogueNode("Dialogue"); });
        btn.text = "Add Dialogue Node";
        toolbar.Add(btn);
        rootVisualElement.Add(toolbar);
    }
    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
