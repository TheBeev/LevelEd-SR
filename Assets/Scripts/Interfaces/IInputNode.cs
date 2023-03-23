using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputNode
{
    string InputNodeName { get; }
    int InputNodeID { get; set; }
    GameObject OutputNodeConnectedObject { get; set; }
    NodeType ThisNodeType { get; }
    void SetUpInput();
}
