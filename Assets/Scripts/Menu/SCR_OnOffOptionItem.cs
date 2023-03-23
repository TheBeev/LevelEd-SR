using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_OnOffOptionItem : MonoBehaviour, IToolOptionMenuItem
{
    [SerializeField] private GameObject optionMenuObject;
    [SerializeField] private OptionActive optionActive;

    private IOnOffToolOption optionMenu;
    private Renderer currentRend;
    private bool bCurrentlySelected;

    void Awake()
    {
        optionMenu = optionMenuObject.GetComponent<IOnOffToolOption>();
        currentRend = GetComponent<Renderer>();
    }

    public void Selected()
    {
        optionMenu.ToggleStatus(optionActive, gameObject);
    }

    public void SelectedToggle()
    {
        bCurrentlySelected = true;
    }

    public void Deselected()
    {
        bCurrentlySelected = false;
    }

    public void CheckMaterials(bool bOptionActive)
    {
        if (bCurrentlySelected)
        {
            if (bOptionActive)
            {
                currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;
            }
            else
            {
                currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterialFaded;
            }
        }
        else
        {
            if (bOptionActive)
            {
                currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
            }
            else
            {
                currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterialFaded;
            }
        }
    }

}

