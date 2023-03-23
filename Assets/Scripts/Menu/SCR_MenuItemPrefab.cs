using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_MenuItemPrefab : MonoBehaviour, IToolMenuItem {

    [SerializeField] private GameObject toolToActivate;
    [SerializeField] private GameObject prefabToSpawn;
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

    public void OnSelected()
    {
        if (!toolToActivate.activeSelf)
        {
            currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;
            gameObject.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
            toolToActivate.GetComponent<SCR_AddPrefab>().PrefabToSpawn = prefabToSpawn;
            toolToActivate.SetActive(true);
            bCurrentlySelected = true;
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

    void Start()
    {
        currentRend = GetComponent<Renderer>();
    }
}
