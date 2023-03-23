using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuPopoutItem : MonoBehaviour, IMenuPopoutItem, IToolMenuItem
{
    [SerializeField] private GameObject parentPopoutMenuObject;
    [SerializeField] private TextMeshProUGUI descriptionTextObject;
    [SerializeField] private string descriptionText;

    [SerializeField] private bool bCloseMenuOnSelection = true;
    public bool CloseMenuOnSelection
    {
        get { return bCloseMenuOnSelection; }
    }

    [SerializeField] private string optionUIName;
    public string OptionUIName
    {
        get { return optionUIName; }
    }

    [SerializeField] private Mesh modelIcon;
    public Mesh ModelIcon
    {
        get { return modelIcon; }
    }

    [SerializeField] private GameObject toolToActivate;
    public GameObject ToolToActivate
    {
        get { return toolToActivate; }
    }

    private IToolMenuItem parentToolMenuItem;
    private IMenuPopout parentPopoutMenu;
    private bool bCurrentlySelected;
    private Renderer currentRend;

    public void OnSelected()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;
        gameObject.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
        parentPopoutMenu.PopoutSelected(toolToActivate, gameObject);
        bCurrentlySelected = true;
    }

    public void Deselected()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
        parentToolMenuItem.Deselected();
        parentPopoutMenu.DeactivatePopout();
        bCurrentlySelected = false;
    }

    public void Highlighted()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.highlightedMenuMaterial;
        descriptionTextObject.text = descriptionText;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void Unhighlighted()
    {
        if (bCurrentlySelected)
        {
            currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;
        }
        else
        {
            currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
        }

        descriptionTextObject.text = "Hover for description";
        transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
    }

    void Awake()
    {
        parentPopoutMenu = parentPopoutMenuObject.GetComponent<IMenuPopout>();
        parentToolMenuItem = parentPopoutMenuObject.GetComponent<IToolMenuItem>();
        currentRend = GetComponent<Renderer>();
    }

}
