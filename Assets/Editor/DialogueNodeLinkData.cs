using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 用于存储Node之间的连接关系, 在GraphView里是通过Port连接起来的,
// 存储的时候要额外创建一个NodeLink
[Serializable]
public class DialogueNodeLinkData
{
	public string baseNodeGuid;
	public string portName;
	public string targetNodeGuid;
}