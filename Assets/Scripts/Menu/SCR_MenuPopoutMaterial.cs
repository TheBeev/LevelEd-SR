using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuPopoutMaterial : MonoBehaviour, IToolMenuItem, IMenuPopout
{

    [SerializeField] private List<GameObject> popoutObjects = new List<GameObject>();
    [SerializeField] private GameObject defaultMenuItemObject;
    [SerializeField] private Material materialToUse;
    [SerializeField] private bool bModelIcon;
    [SerializeField] private MeshFilter iconMeshFilter;
    [SerializeField] private Renderer iconRenderer;
    [SerializeField] private GameObject toolDescriptionText;
    [SerializeField] private TextMeshProUGUI descriptionTextObject;
    [SerializeField] private string descriptionText;

    [SerializeField] private bool bCloseMenuOnSelection = true;
    public bool CloseMenuOnSelection
    {
        get { return bCloseMenuOnSelection; }
    }

    private TextMeshProUGUI currentMenuToolText; //tool currently set for non-popout menu item
    public GameObject ToolToActivate
    {
        get { return currentMenuToolObject; }
    }
    
    private GameObject currentMenuToolObject; //tool currently set for non-popout menu item
    private GameObject popoutMenuItemSelected;
    private bool bPopoutActive;
    private bool bCurrentlySelected;
    private bool bUsingColourValue;
    private SCR_MeshMaterial meshMaterialScript;
    private Renderer currentRend;

    public void OnSelected()
    {
        if (!currentMenuToolObject.activeSelf)
        {
            currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;
            gameObject.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
            currentMenuToolObject.SetActive(true);
            
            if (meshMaterialScript == null)
            {
                meshMaterialScript = currentMenuToolObject.GetComponent<SCR_MeshMaterial>();
            }
            meshMaterialScript.MaterialToUse = materialToUse;
            meshMaterialScript.UsingColourValue = bUsingColourValue;
            bCurrentlySelected = true;
        }
    }

    public void Deselected()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
        currentMenuToolObject.SetActive(false);
        bCurrentlySelected = false;

    }

    public void PopoutSelected(GameObject newToolObject, GameObject newPopoutMenuItemSelected)
    {
        //not needed
    }

    public void PopoutSelected(GameObject newMenuModeObject, GameObject newPopoutMenuItemSelected, Color newColour)
    {
        //empty
    }

    public void PopoutSelected(GameObject newToolObject, GameObject newPopoutMenuItemSelected, Material newMaterialToUse, bool newUsingColourValue)
    {
        popoutMenuItemSelected = newPopoutMenuItemSelected;
        currentMenuToolObject = newToolObject;
        materialToUse = newMaterialToUse;
        bUsingColourValue = newUsingColourValue;

        if (bModelIcon)
        {
            iconMeshFilter.mesh = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().ModelIcon;
            iconRenderer.material = newMaterialToUse;
        }
        else
        {
            currentMenuToolText.text = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().OptionUIName;
        }
        
        OnSelected();
        DeactivatePopout();
    }

    void OnDisable()
    {
        DeactivatePopout();
    }

    public void ActivatePopout()
    {
        if (!bPopoutActive)
        {
            bPopoutActive = true;

            toolDescriptionText.SetActive(false);

            foreach (var item in popoutObjects)
            {
                item.SetActive(true);
            }
        }
    }

    public void DeactivatePopout()
    {
        bPopoutActive = false;

        toolDescriptionText.SetActive(true);

        foreach (var item in popoutObjects)
        {
            item.SetActive(false);
        }

    }

	// Use this for initialization
	void Start ()
    {
        //order of text objects in the menu item matters!
        currentMenuToolText = GetComponentInChildren<TextMeshProUGUI>();

        if (bModelIcon)
        {
            iconMeshFilter.mesh = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().ModelIcon;
        }
        else
        {
            currentMenuToolText.text = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().OptionUIName;
        }

        currentMenuToolObject = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().ToolToActivate;
        currentRend = GetComponent<Renderer>();

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

}
