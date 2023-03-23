using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnOffToolOption
{
    void ToggleStatus(OptionActive optionActive, GameObject referredObject);
}
