using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAxisToolOption
{
    AxisSelected CurrentAxis { get; }
    void SetCurrentAxis(AxisSelected newAxisSelected, GameObject referredObject);
}
