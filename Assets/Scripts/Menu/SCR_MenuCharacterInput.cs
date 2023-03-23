using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterMenuState
{
    Character,
    Done
}

public class SCR_MenuCharacterInput : MonoBehaviour, ICharacterMenuItem, IHighlightMenuItem
{
    [SerializeField] private CharacterMenuState characterOrSpecial = CharacterMenuState.Character;
    public CharacterMenuState CharacterOrSpecial
    {
        get { return characterOrSpecial; }
    }

    [SerializeField] private string characterValue;
    public string CharacterValue
    {
        get { return characterValue; }
    }

    private Renderer currentRend;

    public void Highlighted()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.highlightedMenuMaterial;
        transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
    }

    public void Unhighlighted()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
        transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
    }

    public void Selected()
    {
        //quick highlight
    }

    void Start()
    {
        currentRend = GetComponent<Renderer>();
    }
}
