using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISSAcceptsBool
{
    void AcceptBool(int variableOrder, bool incomingVariable);

    Transform LineEndTransform { get; }
}
