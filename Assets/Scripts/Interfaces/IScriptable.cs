using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScriptable
{
    void ConfigureNewScriptable();
    void ResetNode();
    void Visible(bool toggleOn);
}
