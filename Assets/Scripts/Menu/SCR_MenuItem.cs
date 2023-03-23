using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuItem : MonoBehaviour, IToolMenuItem {

    [SerializeField] private GameObject toolToActivate;
    [SerializeField] private TextMeshProUGUI descriptionTextObject;
    [SerializeField] private string descriptionText;

    [SerializeField] private bool bCloseMenuOnSelection = true;
    public bool CloseMenuOnSelection
    {
        get { return bCloseMenuOnSelection; }
    }

    public GameObject ToolToActivate
    {
        get { return toolToActivate; }
    }

    private bool bCurrentlySelected;
    private Renderer currentRend;

    void Awake()
    {
        currentRend = GetComponent<Renderer>();
    }

    public void OnSelected()
    {
        if (!toolToActivate.activeSelf)
        {
            if (!currentRend)
            {
                currentRend = GetComponent<Renderer>();
            }

            currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;
            gameObject.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
            bCurrentlySelected = true;
            toolToActivate.SetActive(true);
        }
    }

    public void Deselected()
    {
        bCurrentlySelected = false;
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
        toolToActivate.SetActive(false);
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
