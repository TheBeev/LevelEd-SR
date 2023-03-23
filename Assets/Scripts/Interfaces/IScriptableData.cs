using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScriptableData<T>
{
    void ReconfigureScriptableNode(T newData);
    void ReconfigureOutputNodes();
    void StartDataFlow();
    ParentScriptData GetData();
}
