using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuPopout : MonoBehaviour, IToolMenuItem, IMenuPopout
{

    [SerializeField] private List<GameObject> popoutObjects = new List<GameObject>();
    [SerializeField] private GameObject defaultMenuItemObject;
    [SerializeField] private bool bModelIcon;
    [SerializeField] private MeshFilter iconMeshFilter;
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
    private Renderer currentRend;
    

    public void OnSelected()
    {
        if (!currentMenuToolObject.activeSelf)
        {
            currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;
            gameObject.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
            currentMenuToolObject.SetActive(true);
            bCurrentlySelected = true;
        }
    }

    public void Deselected()
    {
        currentMenuToolObject.SetActive(false);
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
        bCurrentlySelected = false;
    }

    public void PopoutSelected(GameObject newToolObject, GameObject newPopoutMenuItemSelected)
    {
        popoutMenuItemSelected = newPopoutMenuItemSelected;
        currentMenuToolObject = newToolObject;  

        if (bModelIcon)
        {
            iconMeshFilter.mesh = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().ModelIcon;
        }
        else
        {
            currentMenuToolText.text = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().OptionUIName;
        }
        
        OnSelected();
        DeactivatePopout();
    }

    public void PopoutSelected(GameObject newToolObject, GameObject newPopoutMenuItemSelected, Color newColourToUse)
    {
        //not needed
    }

    public void PopoutSelected(GameObject newMenuModeObject, GameObject newPopoutMenuItemSelected, Material newMaterial, bool bMaterialUsesColourValue)
    {
        //empty
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

        currentRend = GetComponent<Renderer>();
        currentMenuToolObject = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().ToolToActivate;
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
