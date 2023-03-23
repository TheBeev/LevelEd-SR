using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenuPopoutItem
{
    string OptionUIName { get; }
    Mesh ModelIcon { get; }
    GameObject ToolToActivate { get; }
}
