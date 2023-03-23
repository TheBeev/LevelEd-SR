using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_AxisOptionItem : MonoBehaviour, IToolOptionMenuItem {

    [SerializeField] private GameObject axisOptionMenuObject;
    [SerializeField] private AxisSelected axisOption;

    private IAxisToolOption axisOptionMenu;
    private bool bCurrentlySelected;
    private Renderer currentRend;

    void Awake()
    {
        axisOptionMenu = axisOptionMenuObject.GetComponent<IAxisToolOption>();
        currentRend = GetComponent<Renderer>();
    }

    public void Selected()
    {
        axisOptionMenu.SetCurrentAxis(axisOption, gameObject);
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
