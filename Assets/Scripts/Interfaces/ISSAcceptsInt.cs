using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISSAcceptsInt
{
    void AcceptInt(int varaibleOrder, int incomingVariable);

    Transform LineEndTransform { get; }
}
