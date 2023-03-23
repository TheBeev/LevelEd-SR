using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOutputNode
{
    int InputNodeIDToConnectTo { get; set; }
    NodeType NodeTypeRequired { get; }
    void UpdateInputNodeToConnectTo(int newID, GameObject newTarget, bool newShouldHaveTarget);
    void SetUpOutput();
    void RemoveTarget();
    void Selected(bool currentlySelected);
}
