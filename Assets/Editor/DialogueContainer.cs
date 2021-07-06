using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<DialogueNodeData> nodesData = new List<DialogueNodeData>();
    public List<DialogueNodeLinkData> nodeLinksData = new List<DialogueNodeLinkData>();
}
