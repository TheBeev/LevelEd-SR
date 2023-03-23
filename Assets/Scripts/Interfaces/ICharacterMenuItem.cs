using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterMenuItem
{
    string CharacterValue { get; }
    CharacterMenuState CharacterOrSpecial { get; }
    void Selected();
}
