using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���ڴ洢Node֮������ӹ�ϵ, ��GraphView����ͨ��Port����������,
// �洢��ʱ��Ҫ���ⴴ��һ��NodeLink
[Serializable]
public class DialogueNodeLinkData
{
	public string baseNodeGuid;
	public string portName;
	public string targetNodeGuid;
}