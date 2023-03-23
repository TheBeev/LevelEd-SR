using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToolMenuItem
{
    void OnSelected();
    void Deselected();
    void Highlighted();
    void Unhighlighted();

    GameObject ToolToActivate { get; }
    bool CloseMenuOnSelection { get; }
}
