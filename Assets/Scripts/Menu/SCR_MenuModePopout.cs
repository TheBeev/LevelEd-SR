using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class SCR_MenuModePopout : MonoBehaviour, IToolMenuItem, IMenuPopout
{

    [SerializeField] private ControllerInputs inputMethod = ControllerInputs.RightStick;
    [SerializeField] private List<GameObject> popoutObjects = new List<GameObject>();
    [SerializeField] private GameObject defaultMenuItemObject;
    [SerializeField] private bool bModelIcon;
    [SerializeField] private MeshFilter iconMeshFilter;
    [SerializeField] private GameObject toolModeTextObject;
    [SerializeField] private GameObject toolDescriptionTextObject;
    [SerializeField] private TextMeshProUGUI labelText; //tool currently set for non-popout menu item
    [SerializeField] private bool bAllowShortcuts;

    [SerializeField] private bool bCloseMenuOnSelection = true;
    public bool CloseMenuOnSelection
    {
        get { return bCloseMenuOnSelection; }
    }

    private GameObject currentMenuModeObject; //tool currently set for non-popout menu item
    public GameObject ToolToActivate
    {
        get { return currentMenuModeObject; }
    }

    private GameObject popoutMenuItemSelected;
    private bool bPopoutActive;
    private bool bShortcutInputTriggered;
    private bool bFirstTimeRunning = true;
    private Renderer currentRend;
    private Vector2 inputMovement;

    public void OnSelected()
    {
        //if (!currentMenuModeObject.activeSelf)
        //{
        //gameObject.GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.selectedObjectColour;
        //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.selectedOutlineColour;
        //gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        //currentMenuModeObject.SetActive(true);
        //}
    }

    public void Deselected()
    {

        //gameObject.GetComponent<Renderer>().material.color = SCR_ToolMenuRadial.instance.defaultObjectColour;
        //gameObject.GetComponent<SCR_Outline>().OutlineColor = SCR_ToolMenuRadial.instance.defaultOutlineColour;
        //currentMenuModeObject.SetActive(false);

    }

    public void PopoutSelected(GameObject newMenuModeObject, GameObject newPopoutMenuItemSelected)
    {
        popoutMenuItemSelected.GetComponent<IMenuModeItem>().DeselectedMode();
        popoutMenuItemSelected = newPopoutMenuItemSelected;

        currentMenuModeObject.SetActive(false);
        currentMenuModeObject = newMenuModeObject;
        currentMenuModeObject.SetActive(true);

        if (bModelIcon)
        {
            iconMeshFilter.mesh = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().ModelIcon;
        }
        else
        {
            labelText.text = popoutMenuItemSelected.GetComponent<IMenuPopoutItem>().OptionUIName;
        }

        //OnSelected();
        //DeactivatePopout();
    }

    public void PopoutSelected(GameObject newMenuModeObject, GameObject newPopoutMenuItemSelected, Color newColour)
    {
        //empty
    }
    public void PopoutSelected(GameObject newMenuModeObject, GameObject newPopoutMenuItemSelected, Material newMaterial, bool bMaterialUsesColourValue)
    {
        //empty
    }

    void OnDisable()
    {
        SCR_InputDetection.instance.UnsubscribeFromInput(inputMethod, DoInputDetected);
        //DeactivatePopout();
    }

    public void ActivatePopout()
    {
        if (!bPopoutActive)
        {
            bPopoutActive = true;

            labelText.enabled = false;

            if (toolModeTextObject)
            {
                toolModeTextObject.SetActive(false);
            }

            if (toolDescriptionTextObject)
            {
                toolDescriptionTextObject.SetActive(false);
            }

            foreach (var item in popoutObjects)
            {
                item.SetActive(true);
            }
        }
    }

    public void DeactivatePopout()
    {
        bPopoutActive = false;

        labelText.enabled = true;

        if (toolModeTextObject)
        {
            toolModeTextObject.SetActive(true);
        }

        if (toolDescriptionTextObject)
        {
            toolDescriptionTextObject.SetActive(true);
        }

        foreach (var item in popoutObjects)
        {
            item.SetActive(false);
        }

    }

    public void Highlighted()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.highlightedMenuMaterial;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void Unhighlighted()
    {
        currentRend.sharedMaterial = SCR_ToolMenuRadial.instance.defaultMenuMaterial;
        transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
    }

    // Use this for initialization
    private void OnEnable()
    {
        if (bFirstTimeRunning)
        {
            if (bModelIcon)
            {
                iconMeshFilter.mesh = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().ModelIcon;
            }
            else
            {
                labelText.text = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().OptionUIName;
            }

            currentRend = GetComponent<Renderer>();
            currentMenuModeObject = defaultMenuItemObject.GetComponent<IMenuPopoutItem>().ToolToActivate;
            popoutMenuItemSelected = defaultMenuItemObject;
            popoutMenuItemSelected.GetComponent<MeshRenderer>().sharedMaterial = SCR_ToolMenuRadial.instance.selectedMenuMaterial;

            bFirstTimeRunning = false;
        }

        SCR_InputDetection.instance.SubscribeToInput(inputMethod, DoInputDetected);
    }

    private void DoInputDetected(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputMovement = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            inputMovement = Vector3.zero;
        }
    }

    private void Update()
    {
        if (bAllowShortcuts)
        {
            ShortcutHandling();
        }
    }

    void ShortcutHandling()
    {

        if (inputMovement.x > 0.5f)
        {
            if (!bShortcutInputTriggered)
            {
                ShortcutSwapMode(false);
            }
            bShortcutInputTriggered = true;
        }
        else if (inputMovement.x < -0.5f)
        {
            if (!bShortcutInputTriggered)
            {
                ShortcutSwapMode(true);
            }
            bShortcutInputTriggered = true;
        }
        else
        {
            bShortcutInputTriggered = false;
        }
    }

    public void ShortcutSwapMode(bool bLeft)
    {
        int modeIndex = popoutObjects.IndexOf(popoutMenuItemSelected);

        if (bLeft)
        {
            if ((modeIndex - 1) >= 0)
            {
                popoutObjects[modeIndex - 1].GetComponent<SCR_MenuModeFixedItem>().OnSelected();
            }
            else
            {
                popoutObjects[popoutObjects.Count - 1].GetComponent<SCR_MenuModeFixedItem>().OnSelected();
            }
        }
        else
        {
            if ((modeIndex + 1) <= popoutObjects.Count - 1)
            {
                popoutObjects[modeIndex + 1].GetComponent<SCR_MenuModeFixedItem>().OnSelected();
            }
            else
            {
                popoutObjects[0].GetComponent<SCR_MenuModeFixedItem>().OnSelected();
            }
        }
        
    }

}
